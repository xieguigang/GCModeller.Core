﻿Imports System.Text.RegularExpressions
Imports LANS.SystemsBiology.SequenceModel
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic

Namespace Assembly.MiST2

    <PackageNamespace("MiST2.WebServices")>
    Public Module WebServices

        Const SignalTransductionProfileComment As String = "<!-- Signal transduction profile -->"
        Const SignalTransductionProfileImage As String = "<img src=""/img/tmp/.+"""

        Const MAJOR_MODES As String = "<div class=""major_block"">.+?</div>"

        ''' <summary>
        ''' 使用这个方法进行数据的下载：
        ''' 获取某一个物种基因组内的信号转导网络数据
        ''' 首先，在MisT2网站上搜索菌株名称，得到菌株编号
        ''' 之后在使用所得到的<paramref name="speciesCode">菌株编号</paramref>调用本方法既可以下载数据了
        ''' </summary>
        ''' <param name="speciesCode">MiST2数据库内的物种编码</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Downloads")>
        Public Function DownloadData(speciesCode As String) As MiST2
            Dim pageContent As String = ("http://mistdb.com/bacterial_genomes/summary/" & speciesCode).GET
            Dim Report As MiST2 = New MiST2
            Call ParseGenomeSummary(Report, pageContent)
            pageContent = Mid(pageContent, InStr(pageContent, SignalTransductionProfileComment) + Len(SignalTransductionProfileComment))
            Report.ProfileImageUrl = Regex.Match(pageContent, SignalTransductionProfileImage, RegexOptions.IgnoreCase).Value
            Report.ProfileImageUrl = "http://mistdb.com" & Strings.Split(Mid(Report.ProfileImageUrl, 11), """ ").First
            Report.MiST2Code = speciesCode

            Call Report.Parse(Regex.Match(pageContent, MAJOR_MODES, RegexOptions.Singleline).Value)

            Return Report
        End Function

        Const SUMMARY_ITEM As String = "<th>[a-z ]+</th>[^<]+?<td[^>]*>[^<]+</td>"
        Const GCCONTENT As String = "<td>\d+(\.\d+)?[%]</td>"

        Private Sub ParseGenomeSummary(Profile As MiST2, pageContent As String)
            Dim summary As String = Mid(pageContent, InStr(pageContent, "<!-- Genome summary -->") + 25)
            Dim items = (From m As Match In Regex.Matches(summary, SUMMARY_ITEM, RegexOptions.Singleline + RegexOptions.IgnoreCase) Select m.Value).ToArray
            Dim p As Integer

            Const VALUE_SECTION As String = ">[^<]+</td>"

            Static Dim GetValue As Func(Of String, String) = Function(str As String) Mid(str, 2, Len(str) - 6)

            Profile.Organism = Regex.Match(items(p.MoveNext), VALUE_SECTION).Value
            Profile.Organism = GetValue(Profile.Organism)

            Profile.Taxonomy = Regex.Match(items(p.MoveNext), VALUE_SECTION).Value
            Profile.Taxonomy = GetValue(Profile.Taxonomy)

            Profile.Size = Regex.Match(items(p.MoveNext), VALUE_SECTION).Value
            Profile.Size = GetValue(Profile.Size)

            Profile.Status = Regex.Match(items(p.MoveNext), VALUE_SECTION).Value
            Profile.Status = GetValue(Profile.Status)

            Profile.Replicons = Regex.Match(items(p.MoveNext), VALUE_SECTION).Value
            Profile.Replicons = GetValue(Profile.Replicons)

            Profile.Genes = Regex.Match(items(p.MoveNext), VALUE_SECTION).Value
            Profile.Genes = GetValue(Profile.Genes)

            Profile.Proteins = Regex.Match(items(p.MoveNext), VALUE_SECTION).Value
            Profile.Proteins = GetValue(Profile.Proteins)

            Profile.GC = Regex.Match(summary, GCCONTENT, RegexOptions.IgnoreCase).Value
            Profile.GC = Mid(GetValue(Profile.GC), 4)
        End Sub

        Const CONTENT_RECORD As String = "<input type=""checkbox"".+?</map></div>"

        ''' <summary>
        ''' 获取目标页面内部的所有蛋白质数据
        ''' </summary>
        ''' <param name="url"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Downloads")>
        Public Function Download(url As String) As Transducin()
            If String.IsNullOrEmpty(url) Then
                Return New Transducin() {}
            End If

            Dim pageContent As String = url.GET, proteinCounts As String = GetProteinCounts(pageContent)
            Dim matchs = (From item As Match In Regex.Matches(pageContent, CONTENT_RECORD, RegexOptions.Singleline + RegexOptions.IgnoreCase) Select item.Value).ToArray

            Call $"[MiST2 web_request handler] Loading data from {proteinCounts}...".__DEBUG_ECHO

            Dim proteinArray As List(Of Transducin) = (From strItem As String In matchs Select Match(strItem)).ToList

            '查找NEXT标签所指向的页面
            Const NEXT_PAGE As String = "<a href=""[^>]+?"">Next</a>"
            Dim next_page_url As String = Regex.Match(pageContent, NEXT_PAGE, RegexOptions.IgnoreCase).Value

            If Not String.IsNullOrEmpty(next_page_url) Then
                next_page_url = Regex.Match(next_page_url, "href="".+?""", RegexOptions.IgnoreCase).Value
                next_page_url = "http://mistdb.com" & Mid(next_page_url, 7, Len(next_page_url) - 7)

                Call proteinArray.AddRange(Download(next_page_url))
            End If

            Return proteinArray.ToArray
        End Function

        Public Function Match(strText As String) As Transducin
            Dim Protein As Transducin = New Transducin
            Dim Tokens = Strings.Split(strText, "</td>").Skip(3).ToArray
            Dim p As Integer

            Protein.Identifier = Regex.Match(Tokens(p.MoveNext), ">[^>]+?</").Value
            Protein.Identifier = GetValue(Protein.Identifier)

            Protein.GeneName = Regex.Match(Tokens(p.MoveNext), ">[^>]+?</").Value
            Protein.GeneName = GetValue(Protein.GeneName)

            Protein.Class = Regex.Match(Tokens(p.MoveNext), ">[^>]+?</").Value
            Protein.Class = GetValue(Protein.Class)

            Protein.Inputs = Strings.Split(Mid(Tokens(p.MoveNext), InStr(Tokens(p - 1), ">") + 1), ", ")
            Protein.Outputs = Strings.Split(Mid(Tokens(p.MoveNext), InStr(Tokens(p - 1), ">") + 1), ", ")

            Const IMAGE_SOURCE As String = "<div class=""arch_img_cont""><img src="".+?"""

            Protein.ImageUrl = Regex.Match(Tokens(p.MoveNext), IMAGE_SOURCE, RegexOptions.IgnoreCase).Value
            Protein.ImageUrl = Regex.Match(Protein.ImageUrl, "src="".+?""").Value
            Protein.ImageUrl = "http://mistdb.com" & Mid(Protein.ImageUrl, 6, Len(Protein.ImageUrl) - 6)

            Return Protein
        End Function

        Private Function GetValue(str As String) As String
            str = If(String.IsNullOrEmpty(str), "", Mid(str, 2, Len(str) - 3))
            Return str
        End Function

        Const PROTEIN_COUNTS As String = "<div class=""paged_header""><div class="".+?""><strong>\d+</strong> - <strong>\d+</strong> out of <strong>\d+</strong> protein\(s\)</div>"

        Private Function GetProteinCounts(pageContent As String) As String
            Dim strCount As String = Regex.Match(pageContent, PROTEIN_COUNTS, RegexOptions.Singleline).Value
            Dim Numbers As String() = (From m As Match In Regex.Matches(strCount, "<strong>\d+</strong>", RegexOptions.Singleline) Select m.Value).ToArray
            Dim start = Regex.Match(Numbers(0), "\d+").Value
            Dim [end] = Regex.Match(Numbers(1), "\d+").Value

            Return String.Format("{0} ==> {1}", start, [end])
        End Function
    End Module
End Namespace