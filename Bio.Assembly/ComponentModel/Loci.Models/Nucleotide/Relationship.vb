﻿Imports LANS.SystemsBiology.Assembly.NCBI.GenBank.TabularFormat.ComponentModels

Namespace ComponentModel.Loci

    ''' <summary>
    ''' 描述位点在基因组上面的位置，可以使用<see cref="ToString"/>函数获取得到位置描述
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class Relationship(Of T As I_GeneBrief)
        Public Property Gene As T
        Public Property Relation As  SegmentRelationships

        Sub New()
        End Sub

        Sub New(gene As T, rel As  SegmentRelationships)
            Me.Gene = gene
            Me.Relation = rel
        End Sub

        Public Function ATGDist(loci As NucleotideLocation) As Integer
            Dim ATG As Integer
            Dim s As Integer

            If Gene.Location.Strand = Strands.Forward Then
                ATG = Gene.Location.Left
                s = 1
            Else
                ATG = Gene.Location.Right
                s = -1
            End If

            Dim d1 As Integer = loci.Left - ATG
            Dim d2 As Integer = loci.Right - ATG

            If Math.Abs(d1) < Math.Abs(d2) Then
                Return d1 * s
            Else
                Return d2 * s
            End If
        End Function

        ''' <summary>
        ''' 位置关系的描述信息
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return Relation.LocationDescription(Gene)
        End Function
    End Class
End Namespace