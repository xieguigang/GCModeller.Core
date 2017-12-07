﻿#Region "Microsoft.VisualBasic::fd565b8e2580dd528fa37969e60b7fba, ..\GCModeller\core\Bio.Assembly\Assembly\EBI\ChEBI\EntityModel\XML\Extensions.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Assembly.EBI.ChEBI.XML

    <XmlRoot("ChEBI-DataSet", [Namespace]:="http://gcmodeller.org/core/chebi/dataset.XML")>
    Public Class EntityList

        <XmlElement("chebi-entity")>
        Public Property DataSet As ChEBIEntity()

        Public Function ToSearchModel() As Dictionary(Of Long, ChEBIEntity)
            Dim table As New Dictionary(Of Long, ChEBIEntity)

            For Each chemical As ChEBIEntity In DataSet
                Dim id& = chemical.Address

                If Not table.ContainsKey(id) Then
                    table.Add(id, chemical)
                End If
            Next

            Return table
        End Function

        Public Function AsList() As HashList(Of ChEBIEntity)
            Dim list As New HashList(Of ChEBIEntity)

            For Each chemical As ChEBIEntity In DataSet
                Call list.Add(chemical)
            Next

            Return list
        End Function

        Public Overrides Function ToString() As String
            If DataSet.IsNullOrEmpty Then
                Return "No items"
            Else
                Return $"list of {DataSet.Length} chebi entity: ({DataSet.Take(10).Keys.GetJson}...)"
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function LoadDirectory(folder$) As EntityList
            Return Extensions.Compile(folder)
        End Function
    End Class

    Public Module Extensions

        ''' <summary>
        ''' 将单个的chebi分子数据文件合并在一个大文件之中，方便进行数据的加载
        ''' </summary>
        ''' <param name="DIR$"></param>
        ''' <returns></returns>
        Public Function Compile(DIR$) As EntityList
            Dim list As New Dictionary(Of ChEBIEntity)

            For Each path As String In ls - l - r - "*.XML" <= DIR
                For Each chemical As ChEBIEntity In path.LoadXml(Of ChEBIEntity())
                    If Not list & chemical Then
                        list += chemical
                    End If
                Next
            Next

            Return New EntityList With {
                .DataSet = list _
                    .Values _
                    .ToArray
            }
        End Function
    End Module
End Namespace