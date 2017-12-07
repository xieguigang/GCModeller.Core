﻿#Region "Microsoft.VisualBasic::7a4a93f20c87c77cd3a737161420b066, ..\GCModeller\core\Bio.Assembly\Assembly\UniProt\XML\Model\reference.vb"

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

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Assembly.Uniprot.XML

    Public Class reference

        <XmlAttribute>
        Public Property key As String
        Public Property citation As citation
        <XmlElement>
        Public Property scope As String()
        Public Property source As source

    End Class

    Public Class citation
        <XmlAttribute> Public Property type As String
        <XmlAttribute> Public Property [date] As String
        <XmlAttribute> Public Property db As String
        ''' <summary>
        ''' 期刊名称
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property name As String
        <XmlAttribute> Public Property volume As String
        <XmlAttribute> Public Property first As String
        <XmlAttribute> Public Property last As String

        ''' <summary>
        ''' ``submission``类型的引用的标题可能是空的
        ''' </summary>
        ''' <returns></returns>
        Public Property title As String
        Public Property authorList As person()
        <XmlElement("dbReference")>
        Public Property dbReferences As dbReference()

        Public Overrides Function ToString() As String
            Return title
        End Function
    End Class

    Public Class person
        <XmlAttribute> Public Property name As String

        Public Overrides Function ToString() As String
            Return name
        End Function
    End Class

    Public Class source

        <XmlElement("tissue")>
        Public Property tissues As String()

        Public Overrides Function ToString() As String
            Return tissues.GetJson
        End Function
    End Class
End Namespace