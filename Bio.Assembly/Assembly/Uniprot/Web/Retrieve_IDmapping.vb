﻿Imports System.Collections.Specialized
Imports System.Threading
Imports Microsoft.VisualBasic.Text.HtmlParser

Namespace Assembly.Uniprot.Web

    Public Module Retrieve_IDmapping

        Const yes$ = NameOf(yes)
        Const no$ = NameOf(no)

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="uploadQuery"></param>
        ''' <param name="from"></param>
        ''' <param name="[to]"></param>
        ''' <param name="save$"></param>
        ''' <param name="compress$">
        ''' 假若这个参数为<see cref="yes"/>的话，下载的是一个``*.gz``格式的压缩文件
        ''' </param>
        ''' <param name="format"></param>
        ''' <returns></returns>
        Public Function Mapping(uploadQuery As IEnumerable(Of String),
                                from As IdTypes,
                                [to] As IdTypes,
                                save$,
                                Optional compress$ = yes,
                                Optional format As Formats = Formats.xml) As Dictionary(Of String, String())

            Dim args As New NameValueCollection

            Call args.Add(NameOf(from), from.ToString)
            Call args.Add(NameOf([to]), [to].ToString)
            Call args.Add(NameOf(uploadQuery), uploadQuery.JoinBy(vbLf))

            Dim url$ = "http://www.uniprot.org/uploadlists/"
            Dim html As String = url.PostRequest(args, "http://www.uniprot.org/uploadlists/",)
            Dim query$ = html.HTMLTitle.Split.First
            Dim uid$ = query.Split(":"c).Last

            Call Thread.Sleep(1000)

            ' http://www.uniprot.org/uniprot/
            ' query=yourlist:M20170110ACFE4208EAFA842A78A1B3BA7138A93D9543F8P
            ' sort=yourlist:M20170110ACFE4208EAFA842A78A1B3BA7138A93D9543F8P
            ' columns=yourlist(M20170110ACFE4208EAFA842A78A1B3BA7138A93D9543F8P),isomap(M20170110ACFE4208EAFA842A78A1B3BA7138A93D9543F8P),id,entry%20name,reviewed,protein%20names,genes,organism,length
            url = "http://www.uniprot.org/uniprot/?"
            url &= "query=" & query & "&"
            url &= "sort=" & query & "&"
            url &= $"columns=yourlist({uid}),isomap({uid}),id,entry%20name,reviewed,protein%20names,genes,organism,length"
            html = url.GET()
            html = Strings.Split(html, "UniProtKB Results").Last

            Dim out As New Dictionary(Of String, List(Of String))
            Dim table = html.GetTablesHTML.FirstOrDefault
            Dim rows = table.GetRowsHTML

            For Each row As String In rows.Skip(2)
                Dim columns$() = row.GetColumnsHTML
                Dim queryId$ = columns(1)
                Dim mapId$ = columns(3).GetValue

                If Not out.ContainsKey(queryId) Then
                    Call out.Add(queryId, New List(Of String))
                End If

                Call out(queryId).Add(mapId)
            Next

            url = $"http://www.uniprot.org/uniprot/?sort={query}&desc=&compress={compress}&query={query}&fil=&format={format}&force=yes"

            Try
                Call url.DownloadFile(save)
            Catch ex As Exception
                Call App.LogException(New Exception(url, ex))
            End Try

            Return out.ToDictionary(
                Function(k) k.Key,
                Function(v) v.Value.ToArray)
        End Function
    End Module

    Public Enum Formats
        ''' <summary>
        ''' FASTA (canonical)
        ''' </summary>
        canonical
        ''' <summary>
        ''' FASTA (canonical &amp; isoform)
        ''' </summary>
        isoform
        ''' <summary>
        ''' Tab-separated
        ''' </summary>
        tab
        ''' <summary>
        ''' Text
        ''' </summary>
        txt
        ''' <summary>
        ''' Excel
        ''' </summary>
        xlsx
        ''' <summary>
        ''' GFF
        ''' </summary>
        gff
        ''' <summary>
        ''' XML
        ''' </summary>
        xml
        ''' <summary>
        ''' Mapping Table
        ''' </summary>         
        mappingTable
        ''' <summary>
        ''' RDF/XML
        ''' </summary>
        rdf
        ''' <summary>
        ''' Target List
        ''' </summary>
        list
    End Enum

    Public Enum IdTypes
        ''' <summary>
        ''' UniProtKB
        ''' </summary>
        ACC
        ''' <summary>
        ''' UniRef90
        ''' </summary>
        NF90
    End Enum
End Namespace