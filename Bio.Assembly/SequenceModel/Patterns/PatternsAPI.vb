﻿Imports System.Text
Imports LANS.SystemsBiology.SequenceModel.FASTA
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Linq
Imports System.Runtime.CompilerServices

Namespace SequenceModel.Patterns

    Public Module PatternsAPI

        ''' <summary>
        ''' 简单的统计残基的出现频率
        ''' </summary>
        ''' <param name="Fasta"></param>
        ''' 
        <ExportAPI("NT.Frequency")>
        Public Sub Frequency(<Parameter("Path.Fasta")> Fasta As String, Optional offset As Integer = 0)
            Dim data As PatternModel = Frequency(FastaFile.Read(Fasta))
            Dim doc As New StringBuilder(NameOf(Frequency) & ",")
            Call doc.AppendLine(String.Join(",", DirectCast(data.PWM.First, SimpleSite).Alphabets.Keys.ToArray(Function(c) CStr(c))))
            Call doc.AppendLine(String.Join(vbCrLf, data.PWM.ToArray(Function(obj, i) $"{i + offset },{String.Join(",", obj.EnumerateValues.ToArray(Of String)(Function(oo) CStr(oo)))}")))

            Call doc.ToString.SaveTo(Fasta & ".csv")
        End Sub

        ''' <summary>
        ''' 返回来的数据之中的残基的字符是大写的
        ''' </summary>
        ''' <param name="Fasta"></param>
        ''' <returns></returns>
        <ExportAPI("NT.Frequency")>
        Public Function Frequency(Fasta As FastaFile) As PatternModel
            Return Frequency(source:=Fasta)
        End Function

        ''' <summary>
        ''' Simple function for statics the alphabet frequency in the fasta source. 
        ''' The returns matrix, alphabet key char is Upper case.
        ''' (返回来的数据之中的残基的字符是大写的)
        ''' </summary>
        ''' <param name="source">
        ''' Fasta sequence source, and all of the fasta sequence 
        ''' in this source must in the same length.
        ''' </param>
        ''' <returns></returns>
        <ExportAPI("NT.Frequency")>
        Public Function Frequency(source As IEnumerable(Of FastaToken)) As PatternModel
            Dim len As Integer = source.First.Length
            Dim n As Integer = source.Count
            Dim alphabets As Char() =
                If(source.First.IsProtSource,
                   Polypeptides.ToChar.Values.ToArray, New Char() {"A"c, "T"c, "G"c, "C"c})

            ' Converts the alphabets in the sequence data to upper case.
            Dim fasta As New FastaFile(source.ToArray(Function(x) x.ToUpper))
            Dim LQuery = (From pos As Integer
                          In len.Sequence.AsParallel
                          Select pos,
                              row = (From c As Char
                                     In alphabets
                                     Select c, ' Statics for the alphabet frequency at each column
                                         f = __frequency(fasta, pos, c, n)).ToArray
                          Order By pos Ascending).ToArray
            Dim Model As IEnumerable(Of SimpleSite) =
                From x
                In LQuery.SeqIterator
                Let freq As Dictionary(Of Char, Double) =
                    x.obj.row.ToDictionary(Function(o0) o0.c, Function(o0) o0.f)
                Select New SimpleSite(freq, x.Pos)

            Return New PatternModel(Model)
        End Function

        ''' <summary>
        ''' The conservation percentage (%) Is defined as the number of genomes with the same letter on amultiple sequence alignment normalized to range from 0 to 100% for each site along the chromosome of a specific index genome.
        ''' </summary>
        ''' <returns></returns>
        ''' <param name="index">参考序列在所输入的fasta序列之中的位置，默认使用第一条序列作为参考序列</param>
        <ExportAPI("NT.Variations",
                   Info:="The conservation percentage (%) Is defined as the number of genomes with the same letter on amultiple sequence alignment normalized to range from 0 to 100% for each site along the chromosome of a specific index genome.")>
        Public Function NTVariations(<Parameter("Fasta",
                                                "The fasta object parameter should be the output of mega multiple alignment result. All of the sequence in this parameter should be in the same length.")>
                                     Fasta As FASTA.FastaFile,
                                     <Parameter("Index",
                                                "The index of the reference genome in the fasta object parameter, default value is ZERO (The first sequence as the reference.)")>
                                     Optional index As Integer = Scan0,
                                     Optional cutoff As Double = 0.75) As Double()
            Dim ref As FASTA.FastaToken = Fasta(index)
            Return ref.NTVariations(Fasta, cutoff)
        End Function

        <ExportAPI("NT.Variations",
                   Info:="The conservation percentage (%) Is defined as the number of genomes with the same letter on amultiple sequence alignment normalized to range from 0 to 100% for each site along the chromosome of a specific index genome.")>
        <Extension>
        Public Function NTVariations(ref As FastaToken,
                                     <Parameter("Fasta",
                                                "The fasta object parameter should be the output of mega multiple alignment result. All of the sequence in this parameter should be in the same length.")>
                                     Fasta As FASTA.FastaFile,
                                     Optional cutoff As Double = 0.75) As Double()
            Dim frq As PatternModel = Frequency(Fasta)
            Dim refSeq As String = ref.SequenceData.ToUpper
            Dim var As Double() = refSeq.ToArray(Function(ch, pos) __variation(ch, pos, cutoff, frq))
            Return var
        End Function

        Private Function __variation(ch As Char, index As Integer, cutoff As Double, fr As PatternModel) As Double
            If ch = "-"c Then
                Return 1
            End If

            Dim pos As IPatternSite = fr(index)
            If Array.IndexOf(pos.EnumerateKeys.ToArray, ch) = -1 Then
                Return 1
            End If

            Dim frq As Double = pos(ch)
            Dim var As Double = 1 - frq

            If var < cutoff Then
                var = 0
            End If

            Return var
        End Function

        ''' <summary>
        ''' Statics of the occurence frequency for the specific alphabet at specific 
        ''' column in the fasta source.
        ''' (因为是大小写敏感的，所以参数<see cref="Fasta"/>里面的所有的序列数据都必须是大写的)
        ''' </summary>
        ''' <param name="Fasta"></param>
        ''' <param name="p">The column number.</param>
        ''' <param name="C">Alphabet specific for the frequency statics</param>
        ''' <param name="numOfFasta">The total number of the fasta sequence</param>
        ''' <returns></returns>
        Private Function __frequency(Fasta As IEnumerable(Of FastaToken),
                                     p As Integer,
                                     C As Char,
                                     numOfFasta As Integer) As Double

            Dim LQuery As Integer = (From nt As FastaToken
                                     In Fasta
                                     Let chr As Char = nt.SequenceData(p)
                                     Where C = chr
                                     Select 1).Sum
            Dim f As Double = LQuery / numOfFasta
            Return f
        End Function
    End Module
End Namespace