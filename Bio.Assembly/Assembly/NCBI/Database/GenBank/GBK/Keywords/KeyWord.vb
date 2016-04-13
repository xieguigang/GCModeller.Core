﻿Imports Microsoft.VisualBasic

Namespace Assembly.NCBI.GenBank.GBFF.Keywords

    Public MustInherit Class KeyWord : Inherits NCBI.GenBank.GBFF.I_GBKComponent

        Public Const GBK_FIELD_KEY_REFERENCE As String = "REFERENCE"
        Public Const GBK_FIELD_KEY_COMMENT As String = "COMMENT"
        Public Const GBK_FIELD_KEY_SOURCE As String = "SOURCE"
        Public Const GBK_FIELD_KEY_DEFINITION As String = "DEFINITION"
        Public Const GBK_FIELD_KEY_ACCESSION As String = "ACCESSION"
        Public Const GBK_FIELD_KEY_ORIGIN As String = "ORIGIN"
        Public Const GBK_FIELD_KEY_FEATURES As String = "FEATURES"
        Public Const GBK_FIELD_KEY_VERSION As String = "VERSION"
        Public Const GBK_FIELD_KEY_LOCUS As String = "LOCUS"
        Public Const GBK_FIELD_KEY_KEYWORDS As String = "KEYWORDS"
        Public Const GBK_FIELD_KEY_DBLINK As String = "DBLINK"

        Protected Friend Sub New()
        End Sub

        Protected Friend Shared Sub __trimHeadKey(ByRef values As String())
            If values Is Nothing Then
                values = New String() {}
            Else
                Dim s As String = values(Scan0)
                If s Is Nothing Then
                    s = ""
                Else
                    s = Mid(s, 11).Trim
                End If
                values(0) = s
            End If
        End Sub
    End Class

    Public Class KEYWORDS : Inherits KeyWord
        Implements ICollection(Of String)

        Public Property KeyWordList As List(Of String)

        Public Overrides Function ToString() As String
            If KeyWordList.IsNullOrEmpty Then
                Return "."
            Else
                Return String.Join("; ", KeyWordList.ToArray)
            End If
        End Function

        Public Shared Function __innerParser(str As String()) As KEYWORDS
            Call __trimHeadKey(str)

            If StringHelpers.IsNullOrEmpty(str) OrElse String.Equals(str.First, ".") Then
                Return New KEYWORDS With {
                    .KeyWordList = New List(Of String)
                }
            End If

            str = Strings.Split(String.Join(" ", str), "; ")
            Return New KEYWORDS With {
                .KeyWordList = str.ToList
            }
        End Function

#Region "Implements Generic.ICollection(Of String)"

        Public Sub Add(item As String) Implements ICollection(Of String).Add
            Call KeyWordList.Add(item)
        End Sub

        Public Sub Clear() Implements ICollection(Of String).Clear
            Call KeyWordList.Clear()
        End Sub

        Public Function Contains(item As String) As Boolean Implements ICollection(Of String).Contains
            Return KeyWordList.Contains(item)
        End Function

        Public Sub CopyTo(array() As String, arrayIndex As Integer) Implements ICollection(Of String).CopyTo
            Call KeyWordList.CopyTo(array, arrayIndex)
        End Sub

        Public ReadOnly Property Count As Integer Implements ICollection(Of String).Count
            Get
                Return KeyWordList.Count
            End Get
        End Property

        Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of String).IsReadOnly
            Get
                Return False
            End Get
        End Property

        Public Function Remove(item As String) As Boolean Implements ICollection(Of String).Remove
            Return KeyWordList.Remove(item)
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of String) Implements IEnumerable(Of String).GetEnumerator
            For Each s As String In KeyWordList
                Yield s
            Next
        End Function

        Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
#End Region
    End Class
End Namespace
