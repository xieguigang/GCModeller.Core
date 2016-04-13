﻿Imports System.Text.RegularExpressions

Namespace ComponentModel.Loci

    ''' <summary>
    ''' 核酸链上面的位点片段之间的位置关系的描述
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum SegmentRelationships As Integer
        ''' <summary>
        ''' There is nothing on this location.
        ''' </summary>
        ''' <remarks></remarks>
        Blank = -100
        ''' <summary>
        ''' The loci is on the upstream of the target loci, but part of the loci was overlapping.
        ''' (目标位点和当前的位点重叠在一个，但是目标位点的左端是在当前位点的上游的)
        ''' </summary>
        UpStreamOverlap = 1
        DownStreamOverlap
        Inside
        UpStream
        DownStream
        ''' <summary>
        ''' 比较的目标位点包括了当前的这个位置参照
        ''' </summary>
        Cover
        Equals

        ''' <summary>
        ''' 指定的位点在目标位点的内部的反向序列之上
        ''' </summary>
        InnerAntiSense = 1000
    End Enum

    ''' <summary>
    ''' The direction of this segment on the nucleotide sequence.(片段在DNA链上面的方向或者是否为互补链)
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum Strands As Integer

        ''' <summary>
        ''' The loci site was on the DNA sequence.(这个片段在DNA链的正义链之上)
        ''' </summary>
        ''' <remarks></remarks>
        Forward = 1
        ''' <summary>
        ''' The loci site was on the DNA complement strand.(这个片段在DNA链的互补链之上) 
        ''' </summary>
        ''' <remarks></remarks>
        Reverse = -1
        ''' <summary>
        ''' I really don't know what the direction of the loci site it is.
        ''' </summary>
        ''' <remarks></remarks>
        Unknown = 0
    End Enum
End Namespace