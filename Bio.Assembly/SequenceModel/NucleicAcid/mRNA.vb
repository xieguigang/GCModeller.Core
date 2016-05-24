﻿Imports Microsoft.VisualBasic

Namespace SequenceModel.NucleotideModels

    Public Module mRNA

        ''' <summary>
        ''' 尝试从一段给定的核酸序列之中寻找出可能的最长的ORF阅读框
        ''' </summary>
        ''' <param name="Nt">请注意这个函数总是从左往右进行计算的，所以请确保这个参数是正义链的或者反义链的已经反向互补了</param>
        ''' <param name="ATG"></param>
        ''' <param name="TGA"></param>
        ''' <returns></returns>
        Public Function Putative_mRNA(Nt As String, ByRef ATG As Integer, ByRef TGA As Integer, Optional ByRef ORF As String = "") As Boolean
            ATG = InStr(Nt, "ATG")

            If Not ATG > 0 Then
[NOTHING]:
                ATG = -1
                TGA = -1 '找不到ATG
                Return False
            End If

            Dim TGAList As New List(Of Integer)  '从最后段开始往前数

            For i As Integer = 1 To Len(Nt)
                Dim p As Integer = InStr(i, Nt, "TGA")
                If p > 0 Then
                    Call TGAList.Add(p)
                    i = p
                Else
                    Exit For      '已经再也找不到位点了
                End If
            Next

            If TGAList.IsNullOrEmpty Then
                GoTo [NOTHING]     '找不到任何TGA位点
            Else
                Call TGAList.Reverse()
            End If

            For i As Integer = ATG To Len(Nt)

                ATG = InStr(i, Nt, "ATG")

                If Not ATG > 0 Then
                    GoTo [NOTHING]
                End If

                For Each Point In TGAList  '从最大的开始进行匹配，要满足长度为3的整数倍这个条件，一旦满足则开始进行成分测试，假若通过则认为找到了一个可能的阅读框
                    TGA = Point

                    If TGA - ATG < 20 Then
                        Continue For
                    End If

                    '并且需要两个位点的位置的长度为3的整数倍
                    Dim ORF_Length As Integer = TGA - ATG + 1
                    Dim n As Integer = ORF_Length Mod 3

                    If n = 0 Then  '长度为3的整数倍，  
                        '取出序列和相邻的序列进行GC比较，差异较明显则认为是
                        ORF = Mid(Nt, ATG, ORF_Length + 2)

                        If ORF_Length < 30 Then
                            Continue For
                        End If

                        Dim Adjacent As String = Mid(Nt, 1, ATG)
                        Dim a As Double = NucleotideModels.GCContent(ORF)
                        Dim b As Double = NucleotideModels.GCContent(Adjacent)
                        Dim d As Double = Math.Abs(a - b)
                        Dim accept As Boolean = d > 0.1
#Const DEBUG = 0
#If DEBUG Then
                    Call Console.WriteLine($"[DEBUG {Now.ToString}] ORF({ATG},{TGA})     {NameOf(Nt)}({a}) -->  {NameOf(Adjacent)}({b})   =====> d_gc%={d};   accept? {accept }")
#End If
                        If accept Then
                            Return True
                        End If

                    End If

                Next

            Next

            GoTo [NOTHING]
        End Function
    End Module
End Namespace