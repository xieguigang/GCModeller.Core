﻿Imports Dir = System.String
Imports System.Text

Namespace Assembly.NCBI.CDD

    Partial Class DomainInfo

        Public NotInheritable Class CDDLoader : Implements System.IDisposable

            Protected Friend Cdd, Cog, Kog, Pfam, Prk, Smart, Tigr As DbFile

            Dim Dir As String

            Protected _getDBMethods As Func(Of DbFile)() = New Func(Of DbFile)() {
                AddressOf GetCdd,
                AddressOf GetCog,
                AddressOf GetKog,
                AddressOf GetPfam,
                AddressOf GetPrk,
                AddressOf GetSmart,
                AddressOf GetTigr
            }

            Public Function GetItem(Id As String) As CDD.SmpFile
                Dim LQuery = (From Db As Func(Of DbFile) In _getDBMethods.AsParallel
                              Let DbFile As DbFile = Db()
                              Let SmpItem As CDD.SmpFile = DbFile.ContainsId(Id)
                              Where Not SmpItem Is Nothing
                              Select SmpItem).FirstOrDefault
                Return LQuery
            End Function

            Default Public ReadOnly Property Item(id As String) As CDD.SmpFile
                Get
                    Return GetItem(id)
                End Get
            End Property

            Sub New(CDDDir As Dir)
                Dir = CDDDir
            End Sub

            Public Function GetPrk() As CDD.DbFile
                If Prk Is Nothing Then
                    Prk = Load("Prk")
                End If
                Return Prk
            End Function

            Public Function GetSmart() As CDD.DbFile
                If Smart Is Nothing Then
                    Smart = Load("Smart")
                End If
                Return Smart
            End Function

            Public Function GetTigr() As CDD.DbFile
                If Tigr Is Nothing Then
                    Tigr = Load("Tigr")
                End If
                Return Tigr
            End Function

            Public Function GetCdd() As CDD.DbFile
                If Cdd Is Nothing Then
                    Cdd = Load("Cdd")
                End If
                Return Cdd
            End Function

            Public Function GetCog() As CDD.DbFile
                If Cog Is Nothing Then
                    Cog = Load("Cog")
                End If
                Return Cog
            End Function

            Public Function GetKog() As CDD.DbFile
                If Kog Is Nothing Then
                    Kog = Load("Kog")
                End If
                Return Kog
            End Function

            Public Function GetPfam() As CDD.DbFile
                If Pfam Is Nothing Then
                    Pfam = Load("Pfam")
                End If
                Return Pfam
            End Function

            Public Function Load(DbName As String) As DbFile
                Call $"> Loading database ""{DbName}""...".__DEBUG_ECHO

                Dim FilePath As String = $"{Dir}/{DbName}.xml"
                Dim DbFile As DbFile = FilePath.LoadTextDoc(Of DbFile)(ThrowEx:=False)
                Return DbFile
            End Function

            Public Function LoadFASTA(DbName As String) As LANS.SystemsBiology.SequenceModel.FASTA.FastaFile
                Return SequenceModel.FASTA.FastaFile.Read(String.Format("{0}/{1}.fasta", Dir, DbName))
            End Function

            Public Function GetFastaUrl(DbName As String) As String
                Return String.Format("{0}/{1}.fasta", Dir, DbName)
            End Function

            Public Overrides Function ToString() As String
                Return Dir
            End Function

            Public Shared Widening Operator CType(CddDir As String) As NCBI.CDD.DomainInfo.CDDLoader
                Return New NCBI.CDD.DomainInfo.CDDLoader(CddDir)
            End Operator

#Region "IDisposable Support"
            Private disposedValue As Boolean ' 检测冗余的调用

            ' IDisposable
            Protected Sub Dispose(disposing As Boolean)
                If Not Me.disposedValue Then
                    If disposing Then
                        ' TODO:  释放托管状态(托管对象)。
                    End If

                    ' TODO:  释放非托管资源(非托管对象)并重写下面的 Finalize()。
                    ' TODO:  将大型字段设置为 null。
                End If
                Me.disposedValue = True
            End Sub

            ' TODO:  仅当上面的 Dispose( disposing As Boolean)具有释放非托管资源的代码时重写 Finalize()。
            'Protected Overrides Sub Finalize()
            '    ' 不要更改此代码。    请将清理代码放入上面的 Dispose( disposing As Boolean)中。
            '    Dispose(False)
            '    MyBase.Finalize()
            'End Sub

            ' Visual Basic 添加此代码是为了正确实现可处置模式。
            Public Sub Dispose() Implements IDisposable.Dispose
                ' 不要更改此代码。    请将清理代码放入上面的 Dispose (disposing As Boolean)中。
                Dispose(True)
                GC.SuppressFinalize(Me)
            End Sub
#End Region

        End Class
    End Class
End Namespace