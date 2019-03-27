﻿Imports SMRUCC.genomics.ComponentModel

Namespace Assembly.iGEM

    Public Class iGEMQuery : Inherits WebQuery(Of String)

        Sub New(cache As String)
            Call MyBase.New(AddressOf urlCreator, Function(id) id, AddressOf partListParser, cache)
        End Sub

        Private Shared Function partListParser(text As String, type As Type) As Object

        End Function

        Private Shared Function urlCreator(partId As String) As String
            Return $"http://parts.igem.org/cgi/xml/part.cgi?part={partId}"
        End Function
    End Class
End Namespace