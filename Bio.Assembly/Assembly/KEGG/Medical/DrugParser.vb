﻿Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Extensions
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.genomics.Assembly.KEGG.DBGET.bGetObject
Imports SMRUCC.genomics.ComponentModel.DBLinkBuilder

Namespace Assembly.KEGG.Medical

    Public Module DrugParser

        Public Iterator Function ParseStream(path$) As IEnumerable(Of Drug)
            Dim lines$() = path.ReadAllLines

            For Each pack As String() In lines.Split("///")
                Yield pack.ParseStream.CreateDrugModel
            Next
        End Function

        <Extension>
        Public Function CreateDrugGroupModel(getValue As Func(Of String, String())) As DrugGroup
            Return New DrugGroup With {
                .Entry = getValue("ENTRY").FirstOrDefault.Split.First,
                .Names = getValue("NAME") _
                    .Where(Function(s) Not s.StringEmpty) _
                    .ToArray,
                .Remarks = getValue("REMARKS"),
                .Comments = getValue("COMMENT").JoinBy(" "),
                .Targets = getValue("TARGET"),
                .Metabolism = getValue("METABOLISM") _
                    .ToArray(Function(s) s.GetTagValue(":", trim:=True)),
                .Interaction = getValue("INTERACTION") _
                    .ToArray(Function(s) s.GetTagValue(":", trim:=True)),
                .Members = getValue("MEMBER"),
                .Class = getValue("CLASS").__classGroup
            }
        End Function

        Const DrugAndGroup$ = "DG?\d+"

        <Extension>
        Private Function __classGroup(data$()) As NamedCollection(Of String)()
            If data.IsNullOrEmpty Then
                Return {}
            ElseIf data.Length = 1 Then
                Return {
                    New NamedCollection(Of String) With {
                        .Name = data(Scan0)
                    }
                }
            End If

            Dim out As New List(Of NamedCollection(Of String))
            Dim temp As New List(Of String)
            Dim name$ = data(Scan0)

            For Each line As String In data
                If line.Locates(DrugAndGroup) = 1 Then
                    temp += line
                Else
                    out += New NamedCollection(Of String) With {
                        .Name = name,
                        .Value = temp
                    }

                    temp *= 0
                    name = line
                End If
            Next

            out += New NamedCollection(Of String) With {
                .Name = name,
                .Value = temp
            }

            Return out
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="getValue">数据源的函数指针</param>
        ''' <returns></returns>
        <Extension> Public Function CreateDrugModel(getValue As Func(Of String, String())) As Drug
            Return New Drug With {
                .Entry = getValue("ENTRY").FirstOrDefault.Split.First,
                .Names = getValue("NAME") _
                    .Where(Function(s) Not s.StringEmpty) _
                    .ToArray,
                .Formula = getValue("FORMULA").FirstOrDefault,
                .Exact_Mass = Val(getValue("EXACT_MASS").FirstOrDefault),
                .Mol_Weight = Val(getValue("MOL_WEIGHT").FirstOrDefault),
                .Remarks = getValue("REMARKS"),
                .DBLinks = getValue("DBLINKS") _
                    .Select(AddressOf DBLink.FromTagValue) _
                    .ToArray,
                .Activity = getValue("ACTIVITY").JoinBy(" "),
                .Atoms = __atoms(getValue("ATOM")),
                .Bounds = __bounds(getValue("BOUND")),
                .Comments = getValue("COMMENT"),
                .Targets = getValue("TARGET"),
                .Metabolism = getValue("METABOLISM") _
                    .ToArray(Function(s) s.GetTagValue(":", trim:=True)),
                .Interaction = getValue("INTERACTION") _
                    .ToArray(Function(s) s.GetTagValue(":", trim:=True)),
                .Source = getValue("SOURCE") _
                    .ToArray(Function(s) s.StringSplit(",\s+")) _
                    .IteratesALL _
                    .ToArray
            }
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="lines$"></param>
        ''' <param name="ref">假设参考文献都是在每一个小节最末尾的部分</param>
        ''' <returns></returns>
        <Extension> Friend Function ParseStream(lines$(), Optional ref As Reference() = void) As Func(Of String, String())
            Dim list As New Dictionary(Of NamedValue(Of List(Of String)))
            Dim tag$ = ""  ' 在这里使用空字符串，如果使用Nothing空值的话，添加字典的时候出发生错误
            Dim values As New List(Of String)
            Dim add = Sub()
                          ' 忽略掉original，bracket这些分子结构参数，因为可以很方便的从ChEBI数据库之中获取得到
                          If Not list.ContainsKey(tag) Then
                              list += New NamedValue(Of List(Of String)) With {
                                  .Name = tag,
                                  .Value = values
                              }
                          End If
                      End Sub

            Dim i As int = Scan0
            Dim line As New Value(Of String)

            Do While i < lines.Length
                Dim s$ = Mid(line = lines(++i), 1, 12).StripBlank

                If s = "REFERENCE" Then
                    ' 已经到小节的末尾了
                    Dim refList As New List(Of Reference)

                    ' 将前面的数据给添加完
                    Call add()

                    lines = lines.Skip(i).ToArray

                    For Each r As String() In lines.Split(Function(str) InStr(str, "REFERENCE") = 1, includes:=True)
                        refList += New Reference(meta:=r)
                    Next

                    ref = refList
                End If

                If s.StringEmpty Then
                    values += (+line).Trim
                Else
                    ' 切换到新的标签
                    Call add()

                    tag = s
                    values = New List(Of String)
                    values += Mid(line, 12).StripBlank
                End If
            Loop

            ' 还会有剩余的数据的，在这里将他们添加上去
            Call add()

            Dim getValue = Function(KEY$) As String()
                               If list.ContainsKey(KEY) Then
                                   Return list(KEY).Value
                               Else
                                   Return {}
                               End If
                           End Function
            Return getValue
        End Function

        Private Function __atoms(lines$()) As Atom()
            If lines.IsNullOrEmpty OrElse
                lines(Scan0).ParseInteger = 0 Then
                Return {}
            End If

            Dim out As New List(Of Atom)

            For Each line$ In lines.Skip(1)
                Dim t$() = line.StringSplit("\s+", True)

                out += New Atom With {
                    .index = Val(t(0)),
                    .Formula = t(1),
                    .Atom = t(2),
                    .M = Val(t(3)),
                    .Charge = Val(t(4)),
                    .Edit = t.Get(5)
                }
            Next

            Return out
        End Function

        Private Function __bounds(lines$()) As Bound()
            If lines.IsNullOrEmpty OrElse
                lines(Scan0).ParseInteger = 0 Then
                Return {}
            End If

            Dim out As New List(Of Bound)

            For Each line$ In lines.Skip(1)
                Dim t$() = line.StringSplit("\s+", True)

                out += New Bound With {
                    .index = Val(t(0)),
                    .a = Val(t(1)),
                    .b = Val(t(2)),
                    .N = Val(t(3)),
                    .Edit = t.Get(4)
                }
            Next

            Return out
        End Function
    End Module
End Namespace