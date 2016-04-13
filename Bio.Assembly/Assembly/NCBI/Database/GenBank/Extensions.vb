﻿Imports LANS.SystemsBiology.Assembly.NCBI.GenBank.GBFF.Keywords
Imports System.Text.RegularExpressions
Imports System.Text
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports LANS.SystemsBiology.Assembly.NCBI.GenBank.GBFF.Keywords.FEATURES.Nodes
Imports LANS.SystemsBiology.ComponentModel
Imports LANS.SystemsBiology.Assembly.NCBI.GenBank.TabularFormat
Imports LANS.SystemsBiology.ComponentModel.Loci
Imports LANS.SystemsBiology.Assembly.NCBI.GenBank.TabularFormat.ComponentModels
Imports LANS.SystemsBiology.SequenceModel

Namespace Assembly.NCBI.GenBank

    <PackageNamespace("NCBI.Genbank.Extensions", Publisher:="amethyst.asuka@gcmodeller.org")>
    Public Module Extensions

        <ExportAPI("ToGff"), Extension>
        Public Function ToGff(gb As GBFF.File) As TabularFormat.GFF
            Dim Gff As New TabularFormat.GFF With {
                .Date = gb.Locus.UpdateTime,
                .Features = gb.Features.ToArray(Function(x) x.ToGff),
                .GffVersion = 3,
                .SeqRegion = New SeqRegion With {
                      .AccessId = gb.Accession.AccessionId,
                      .Start = 1,
                      .Ends = gb.Origin.SequenceData.Length
                },
                .Type = "DNA"
            }
            Return Gff
        End Function

        <ExportAPI("Locus.Maps"), Extension>
        Public Function LocusMaps(gb As GenBank.GBFF.File) As Dictionary(Of String, String)
            Dim LQuery = (From x As GBFF.Keywords.FEATURES.Nodes.Feature
                          In gb.Features._innerList
                          Let locus As String = x.Query(FeatureQualifiers.locus_tag)
                          Where Not String.IsNullOrEmpty(locus)
                          Select locus,
                              loci = x.Location.ToString
                          Group By loci Into Group).ToArray
            Dim hash = LQuery.ToDictionary(Function(x) x.loci, Function(x) x.Group.First.locus)
            Return hash
        End Function

        <ExportAPI("Locus.Maps"), Extension>
        Public Function LocusMaps(PTT As PTT) As Dictionary(Of String, String)
            Dim LQuery = (From x As GeneBrief
                          In PTT.GeneObjects
                          Let locus As String = x.Synonym
                          Where Not String.IsNullOrEmpty(locus)
                          Select locus,
                              loci = x.Location.__lociUid
                          Group By loci Into Group).ToArray
            Dim hash = LQuery.ToDictionary(Function(x) x.loci, Function(x) x.Group.First.locus)
            Return hash
        End Function

        <Extension> Private Function __lociUid(loci As NucleotideLocation) As String
            Call loci.Normalization()

            If loci.Strand = Strands.Forward Then
                Return $"{loci.Left}..{loci.Right}"
            Else
                Return $"complement({loci.Left()}..{loci.Right()})"
            End If
        End Function

        <ExportAPI("Read.PTT")>
        Public Function LoadPTT(path As String) As PTT
            Return PTT.Load(path)
        End Function

        <Extension> Public Function GetObjects(Of TGene As I_GeneBrief)(source As IEnumerable(Of TGene), site As Integer, direction As Strands) As TGene()
            Dim Data As TGene()

            If direction = Strands.Reverse Then
                Data = (From ItemGene As TGene
                        In source
                        Where ItemGene.Location.Strand = Strands.Reverse
                        Select ItemGene).ToArray
            ElseIf direction = Strands.Forward Then
                Data = (From ItemGene As TGene
                        In source
                        Where ItemGene.Location.Strand = Strands.Forward
                        Select ItemGene).ToArray
            Else
                Data = source.ToArray
            End If

            Dim LQuery = (From item As TGene
                          In Data
                          Where item.Location.ContainSite(site)
                          Select item).ToArray
            Return LQuery
        End Function

        ''' <summary>
        ''' Export protein sequence with full annotation.
        ''' </summary>
        ''' <param name="Gbk"></param>
        ''' <returns></returns>
        <ExportAPI("Protein.Export", Info:="Export protein sequence with full annotation.")>
        <Extension> Public Function ExportProteins(Gbk As NCBI.GenBank.GBFF.File) As SequenceModel.FASTA.FastaFile
            Dim LQuery = From feature As FEATURES.Nodes.Feature
                         In Gbk.Features
                         Where String.Equals(feature.KeyName, "CDS")
                         Let attrs As String() = New String() {
                             "gi",
                             Gbk.Accession.ToString,
                             "gb",
                             feature.Query("protein_id"),
                             feature.Query("locus_tag"),
                             feature.Query("gene"),
                             feature.Location.ToString,
                             feature.Query("product")
                         }
                         Select New SequenceModel.FASTA.FastaToken With {
                             .Attributes = attrs,
                             .SequenceData = feature.Query("translation")
                         } '
            Dim Fasta As SequenceModel.FASTA.FastaFile =
                CType(LQuery.ToArray, SequenceModel.FASTA.FastaFile)
            Return Fasta
        End Function

        ''' <summary>
        ''' Locus_tag Product_Description
        ''' </summary>
        ''' <param name="gb"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        '''
        <ExportAPI("Protein.Export_Short", Info:="Short fasta title.")>
        <Extension> Public Function ExportProteins_Short(gb As NCBI.GenBank.GBFF.File,
                                                         <Parameter("locusId.Only")>
                                                         Optional OnlyLocusTag As Boolean = False) As FASTA.FastaFile
            Dim LQuery = From feature As FEATURES.Nodes.Feature
                         In gb.Features
                         Where String.Equals(feature.KeyName, "CDS")
                         Select feature.__protShort(OnlyLocusTag) '
            Dim Fasta As FASTA.FastaFile = New FASTA.FastaFile(LQuery)
            Return Fasta
        End Function

        ''' <summary>
        ''' 假若是新注释的基因组还没有基因号，则默认使用位置来做唯一标示
        ''' </summary>
        ''' <param name="feature"></param>
        ''' <param name="onlyLocusTag"></param>
        ''' <returns></returns>
        <Extension> Private Function __protShort(feature As FEATURES.Nodes.Feature, onlyLocusTag As Boolean) As SequenceModel.FASTA.FastaToken
            Dim product As String = feature.Query("product")
            If product Is Nothing Then
                product = ""
            End If
            Dim locusId As String = feature.Query("locus_tag")
            If locusId Is Nothing Then
                locusId = feature.Location.UniqueId
            End If
            Dim ORF_transl As String = feature.Query("translation")
            Dim attrs As String() = If(Not onlyLocusTag, {locusId & " " & product}, {locusId})
            Dim fa As New SequenceModel.FASTA.FastaToken With {
                .Attributes = attrs,
                .SequenceData = ORF_transl
            } '
            Return fa
        End Function

        ''' <summary>
        '''
        ''' </summary>
        ''' <returns>{locus_tag, gene}</returns>
        ''' <remarks></remarks>
        '''
        <ExportAPI("Export.GeneList")>
        <Extension> Public Function GeneList(Gbk As NCBI.GenBank.GBFF.File) As KeyValuePair(Of String, String)()
            Dim GQuery As IEnumerable(Of GBFF.Keywords.FEATURES.Nodes.Feature) =
                From feature In Gbk.Features
                Where String.Equals(feature.KeyName, "gene")
                Select feature 'Gene list query
            Dim AQuery = From locusTag
                         In GQuery.ToArray
                         Select New KeyValuePair(Of String, String)(locusTag.Query("locus_tag"), locusTag.Query("gene")) '
            Return AQuery.ToArray
        End Function

        <ExportAPI("Export.16SrRNA")>
        <Extension> Public Function _16SribosomalRNA(Gbk As NCBI.GenBank.GBFF.File) As FEATURES.Nodes.Feature
            Dim LQuery = From feature In Gbk.Features.AsParallel
                         Where String.Equals(feature.KeyName, "rRNA") AndAlso InStr(feature.Query("product"), "16S ribosomal RNA")
                         Select feature '
            Return LQuery.First
        End Function
    End Module
End Namespace