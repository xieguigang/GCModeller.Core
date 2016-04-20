﻿Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.Language.UnixBash

Namespace SequenceModel.FASTA.Reflection

    ''' <summary>
    ''' 對象模塊將數據庫中的一條記錄轉換為一條FASTA序列對象
    ''' </summary>
    ''' <remarks></remarks>
    ''' 
    <PackageNamespace("Fasta.Tools")>
    Public Module FastaExportMethods

        ''' <summary>
        ''' 将某一个FASTA序列集合中的序列进行互补操作，对于蛋白质序列，则返回空值
        ''' </summary>
        ''' <param name="FASTA2"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Complement"), Extension>
        Public Function Complement(FASTA2 As FastaFile) As FastaFile
            Dim LQuery = From FASTA In FASTA2.AsParallel
                         Let cpFASTA = FastaToken.Complement(FASTA)
                         Where Not cpFASTA Is Nothing
                         Select cpFASTA
                         Order By cpFASTA.ToString Ascending  '
            Return New FastaFile(LQuery)
        End Function

        ''' <summary>
        ''' 将序列首尾反转
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Reverse")>
        <Extension>
        Public Function Reverse(fa As FastaFile) As FastaFile
            Dim LQuery = From FASTA As FastaToken In fa.AsParallel
                         Let rvFASTA = FASTA.Reverse
                         Select rvFASTA
                         Order By rvFASTA.ToString Ascending '
            Return New FastaFile(LQuery)
        End Function

        <ExportAPI("Merge", Info:="Merge the fasta sequence file from a file list.")>
        <Extension>
        Public Function Merge(list As IEnumerable(Of String), Trim As Boolean) As FastaFile
            Dim MergeFa = (From file As String In list.AsParallel Select FastaFile.Read(file)).MatrixToList

            If Trim Then
                MergeFa = (From fa As FastaToken
                           In MergeFa.AsParallel
                           Let attrs As String() = New String() {fa.Attributes.First.Split.First}
                           Select fa.InvokeSet(NameOf(fa.Attributes), attrs)).ToList
            End If

            MergeFa = (From fa As FastaToken
                       In MergeFa.AsParallel
                       Select fa.FastaTrimCorrupt).ToList
            MergeFa = (From fa As FastaToken
                       In MergeFa.AsParallel
                       Where Not String.IsNullOrEmpty(fa.SequenceData)
                       Select fa).ToList

            Call Console.Write(".")

            Dim fasta As New FastaFile(MergeFa)
            Return fasta
        End Function

        ''' <summary>
        ''' Only merge fasta files in the top level directory.
        ''' </summary>
        ''' <param name="inDIR"></param>
        ''' <param name="trim"></param>
        ''' <returns></returns>
        <ExportAPI("Merge", Info:="Merge the fasta sequence file from a directory.")>
        Public Function Merge(inDIR As String, trim As Boolean) As FastaFile
            Dim files As IEnumerable(Of String) = ls - l - wildcards("*.fa", "*.fsa", "*.fas", "*.fasta") <= inDIR
            Return files.Merge(trim)
        End Function

        Public Function Merge(inDIR As String, ext As String, trim As Boolean) As FastaFile
            Dim files As IEnumerable(Of String) = ls - l - wildcards(ext) <= inDIR
            Return files.Merge(trim)
        End Function

        Const HTML_CHARS As String = "</>!\[].+:()0123456789"

        ''' <summary>
        ''' 有些从KEGG上面下载下来的数据会因为解析的问题出现错误，在这里判断是否有这种错误
        ''' </summary>
        ''' <param name="fa"></param>
        ''' <returns></returns>
        <ExportAPI("Fasta.Corrupted?")>
        <Extension> Public Function FastaCorrupted(fa As FastaToken) As Boolean
            Return __seqCorrupted(fa.SequenceData)
        End Function

        Private Function __seqCorrupted(seq As String) As Boolean
            Dim LQuery As Integer = (From x As Char
                                     In seq
                                     Where HTML_CHARS.IndexOf(x) > -1
                                     Select 1000).FirstOrDefault
            Return LQuery > 0
        End Function

        Const LOCUS_ID As String = "[a-z]+_?\d+"
        Const KEGG_LOCUS As String = "[a-z]{3,5}[:]" & LOCUS_ID

        ''' <summary>
        ''' 第一个字符肯定是M
        ''' </summary>
        ''' <param name="fa"></param>
        ''' <returns></returns>
        <ExportAPI("Fasta.Removes.Corruption")>
        <Extension>
        Public Function FastaTrimCorrupt(fa As FastaToken) As FastaToken
            Dim seq As String = fa.SequenceData
            Dim isCorrupted As Boolean
            Dim n As Integer

            Do While __seqCorrupted(seq)
                Dim i As Integer = InStr(seq, "M", CompareMethod.Text)
REDO:           seq = Mid(seq, i)
                isCorrupted = True

                If i = 0 OrElse n > fa.Length Then
                    Call $"ERROR__{fa.SequenceData}  =>  {seq}".__DEBUG_ECHO
                    ' seq = ""
                    Exit Do
                Else
                    If i = 1 Then
                        i = 2
                        GoTo REDO
                    End If

                    n += 1
                End If
            Loop

            Dim locus As String = Regex.Match(fa.SequenceData, KEGG_LOCUS, RegexOptions.IgnoreCase).Value
            If String.IsNullOrEmpty(locus) Then
                locus = Regex.Match(fa.SequenceData, LOCUS_ID, RegexOptions.IgnoreCase).Value
            End If
            If String.IsNullOrEmpty(locus) Then
                locus = fa.Title
            End If

            If isCorrupted Then
                Call $"{fa.ToString} was corrupted, automatically corrected as {locus}!".__DEBUG_ECHO
            End If

            Return New FastaToken With {
                .Attributes = {locus},
                .SequenceData = seq
            }
        End Function

        <ExportAPI("Read.Fasta")>
        Public Function Load(path As String) As FastaFile
            Return FastaFile.Read(path)
        End Function

        <ExportAPI("Read.FastaToken")>
        Public Function LoadFastaToken(path As String) As FastaToken
            Return FastaToken.Load(path)
        End Function

        Public Function Export(Of T As I_FastaObject)(source As IEnumerable(Of T)) As FastaFile
            Dim SchemaCache As SchemaCache = New SchemaCache(GetType(T))
            Dim LQuery = (From objItem As T
                          In source
                          Let fsa As FastaToken = Export(objItem, SchemaCache)
                          Where Not fsa Is Nothing
                          Select fsa).ToArray
            Return LQuery
        End Function

        Public Function Export(Of TFsaObject As I_FastaObject)(fa As TFsaObject) As FastaToken
            If String.IsNullOrEmpty(fa.GetSequenceData) Then
                Return Nothing
            End If

            Dim SchemaCache As SchemaCache = New SchemaCache(GetType(TFsaObject))
            Dim fsa As FastaToken = Export(fa, SchemaCache)
            Return fsa
        End Function

        Private Function Export(objItem As I_FastaObject, SchemaCache As SchemaCache) As FastaToken
            If String.IsNullOrEmpty(SchemaCache.TitleFormat) Then
                Dim stringItems = (From pairItem As KeyValuePair(Of FastaAttributeItem, System.Reflection.PropertyInfo)
                                   In SchemaCache.attributes
                                   Let value As String = pairItem.Value.GetValue(objItem).ToString
                                   Select If(String.IsNullOrEmpty(pairItem.Key.Precursor), New String() {value}, New String() {pairItem.Key.Precursor, value})).ToArray
                Dim Fsa As FastaToken = New FastaToken With {
                    .SequenceData = objItem.GetSequenceData,
                    .Attributes = stringItems.MatrixToVector
                }
                Return Fsa
            Else
                Dim stringItems = (From pairItem As KeyValuePair(Of FastaAttributeItem, System.Reflection.PropertyInfo)
                                   In SchemaCache.attributes
                                   Let value As String = pairItem.Value.GetValue(objItem).ToString
                                   Select If(String.IsNullOrEmpty(pairItem.Key.Format), value, String.Format(pairItem.Key.Format, value))).ToArray
                Dim Title As String = String.Format(SchemaCache.TitleFormat, stringItems)
                Dim Fsa As FastaToken = New FastaToken With {
                    .SequenceData = objItem.GetSequenceData,
                    .Attributes = Title.Split(CChar("|"))
                }
                Return Fsa
            End If
        End Function

        Friend Class SchemaCache
            Public Property TitleFormat As String
            Public Property attributes As KeyValuePair(Of FastaAttributeItem, System.Reflection.PropertyInfo)()

            Sub New(TypeInfo As System.Type)
                TitleFormat = GetTitleFormat(TypeInfo)
                attributes = (From propertyInfo As System.Reflection.PropertyInfo
                              In TypeInfo.GetProperties()
                              Let custAttr As Object() = propertyInfo.GetCustomAttributes(FastaAttributeItem.FsaAttributeItem, True)
                              Where Not custAttr.IsNullOrEmpty
                              Let pairItem = New KeyValuePair(Of FastaAttributeItem, System.Reflection.PropertyInfo)(DirectCast(custAttr.First(), FastaAttributeItem), propertyInfo)
                              Select pairItem
                              Order By pairItem.Key.Index Ascending).ToArray
            End Sub

            Public Shared Function GetTitleFormat(TypeInfo As System.Type) As String
                Dim attrs As Object() = TypeInfo.GetCustomAttributes(FastaObject.FsaTitle, True)
                If attrs.IsNullOrEmpty Then
                    Return ""
                Else
                    Dim TitleFormat As String = DirectCast(attrs.First, FastaObject).Format
                    Return TitleFormat
                End If
            End Function
        End Class
    End Module
End Namespace