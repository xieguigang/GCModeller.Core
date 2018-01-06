﻿Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.genomics.ComponentModel
Imports SMRUCC.genomics.ComponentModel.Loci

Namespace ContextModel.Operon

    ''' <summary>
    ''' To evaluate the contribution of selected features in operon prediction, we have calculated 
    ''' the numerical values of the features, And then used these values individually And in combination 
    ''' to train a classifier. The features used in our study are
    ''' 
    ''' + (i)   the intergenic distance, 
    ''' + (ii)  the conserved gene neighborhood, 
    ''' + (iii) distances between adjacent genes' phylogenetic profiles, 
    ''' + (iv)  the ratio between the lengths of two adjacent genes And 
    ''' + (v)   frequencies Of specific DNA motifs in the intergenic regions.
    ''' </summary>
    Public Module FeatureScores

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function IntergenicDistance(upstream As NucleotideLocation, downstream As NucleotideLocation) As Integer
            If upstream.Strand <> downstream.Strand Then
                Throw New Exception("Invalid strand data!")
            Else
                If upstream.Strand = Strands.Forward Then
                    Return (downstream.Left - (upstream.Right + 1))
                Else
                    Return (upstream.Left - (downstream.Right + 1))
                End If
            End If
        End Function

        ''' <summary>
        ''' 这个函数定义了基因i和基因j在某一个基因组之中是相邻的概率高低
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function NeighborhoodConservation(Of T As IGeneBrief)(gi$, gj$, genomes As GenomeContext(Of T)(), P As Dictionary(Of String, Double)) As Double
            Dim S = -(Aggregate Gk In genomes Let log = Math.Log(L(gi, gj, G:=Gk, P:=P)) Into Sum(log))
            Return S
        End Function

        ''' <summary>
        ''' where ``L(gi, gj, Gk)`` is the loglikelihood of a gene pair to be neighbors in the kth genome Gk. 
        ''' The log-likelihood score Is computed as the probability that gi And gj are neighbors within a distance dk(i,j) In Gk, 
        ''' Or L(gi, gj, Gk) = log Pij; Pij Is defined as follows:
        ''' 
        ''' (i)   Pij = (1-pik)(1-pjk), if both genes are absent from genome Gk,
        ''' (ii)  Pij = (1-pik) pjk, if only gene j Is present In genome Gk,
        ''' (iii) Pij = pik (1-pjk), if only gene i Is present In genome Gk,
        ''' (iv)  Pij = (pikpjkdk(i, j) (2Nk-dk(i,j)-1))/(Nk(Nk-1)), If genes i And j are present In genome Gk.
        ''' 
        ''' dk(ij) Is the number of genes between gi And gj; Nk Is the number Of genes In genome Gk; And pik Is the probability 
        ''' that gene gi Is present In genome Gk.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="gi$"></param>
        ''' <param name="gj$"></param>
        ''' <param name="G"></param>
        ''' <param name="P"></param>
        ''' <returns></returns>
        Public Function L(Of T As IGeneBrief)(gi$, gj$, G As GenomeContext(Of T), P As Dictionary(Of String, Double)) As Double
            If G.Absent(gi) AndAlso G.Absent(gj) Then
                Return (1 - P(gi)) * (1 - P(gj))
            ElseIf Not G.Absent(gi) AndAlso Not G.Absent(gj) Then
                Dim d = G.Delta(gi, gj)
                Dim N = G.N
                Return (P(gi) * P(gj) * d * (2 * N - d - 1)) / (N * (N - 1))
            ElseIf Not G.Absent(gj) Then
                Return (1 - P(gi)) * P(gj)
            ElseIf Not G.Absent(gi) Then
                Return P(gi) * (1 - P(gj))
            Else
                ' ???
                Throw New NotImplementedException("This exception is never happend!")
            End If
        End Function

        ''' <summary>
        ''' ``pik`` was calculated as the frequency Of gene ``gi`` present In the phylum that ``Gk`` belongs to.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="i$">The feature ``gi``</param>
        ''' <param name="genomes">phylum, the collection of ``Gk``</param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function P(Of T As IGeneBrief)(i$, genomes As GenomeContext(Of T)()) As Double
            Return genomes _
                .Where(Function(g) Not g.GetByFeature(i).IsNullOrEmpty) _
                .Count / genomes.Length
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function P(Of T As IGeneBrief)(genomes As GenomeContext(Of T)()) As Dictionary(Of String, Double)
            Return genomes _
                .Select(Function(g) g.AllFeatureKeys) _
                .IteratesALL _
                .Distinct _
                .OrderBy(Function(f) f) _
                .ToDictionary(Function(feature) feature,
                              Function(i) P(i, genomes))
        End Function
    End Module
End Namespace