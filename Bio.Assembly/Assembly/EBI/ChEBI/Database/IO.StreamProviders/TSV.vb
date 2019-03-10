﻿#Region "Microsoft.VisualBasic::1ac493c578bb8f4b52b1cc32444db9c0, Bio.Assembly\Assembly\EBI\ChEBI\Database\IO.StreamProviders\TSV.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:

    '     Class TSVTables
    ' 
    '         Properties: FileName, TsvFiles
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: __lazyLoadData, GetChemicalData, GetDatabaseAccessions, GetInChI, GetNames
    '         Structure FileNames
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.genomics.Assembly.EBI.ChEBI.Database.IO.StreamProviders.Tsv.Tables

Namespace Assembly.EBI.ChEBI.Database.IO.StreamProviders.Tsv

    ''' <summary>
    ''' 可以使用这个对象从ChEBI的ftp文件夹下载结果之中加载相应的表数据
    ''' </summary>
    Public Class TSVTables : Inherits ComponentModel.TabularLazyLoader

        Public Structure FileNames

            ReadOnly autogenerated_structures$, vertice$, relation$, reference$, ontology$, names$, default_structures$, database_accession$, compounds$, compound_origins$, comments$, chemical_data$, chebiId_inchi$, chebi_uniprot$

            ''' <summary>
            ''' 
            ''' </summary>
            ''' <param name="DIR$"></param>
            ''' <remarks>
            ''' 这个结构对象之中的这些域分别对应着ChEBI的ftp文件夹之中的每一个tsv数据表格文件，
            ''' 可以修改域名称的大小写，但是不建议修改符号或者其他的。如果需要运行于Linux环境
            ''' 之中，大小写也不建议进行修改，因为这个构造函数之中是直接通过NameOf操作符来获取
            ''' 文件名的
            ''' </remarks>
            Sub New(DIR$)
                Dim getName = Function(name$) $"{DIR}/{name}.tsv"

                autogenerated_structures = getName(NameOf(autogenerated_structures))
                vertice = getName(NameOf(vertice))
                relation = getName(NameOf(relation))
                reference = getName(NameOf(reference))
                ontology = getName(NameOf(ontology))
                names = getName(NameOf(names))
                default_structures = getName(NameOf(default_structures))
                database_accession = getName(NameOf(database_accession))
                compounds = getName(NameOf(compounds))
                compound_origins = getName(NameOf(compound_origins))
                comments = getName(NameOf(comments))
                chemical_data = getName(NameOf(chemical_data))
                chebiId_inchi = getName(NameOf(chebiId_inchi))
                chebi_uniprot = getName(NameOf(chebi_uniprot))
            End Sub
        End Structure

        Public ReadOnly Property FileName As FileNames

        ''' <summary>
        ''' A complete tsv file list in the chebi ftp data directory.
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property TsvFiles As String() = {
            "autogenerated_structures.tsv",
            "vertice.tsv",
            "relation.tsv",
            "reference.tsv",
            "ontology.tsv",
            "names.tsv",
            "default_structures.tsv",
            "database_accession.tsv",
            "compounds.tsv",
            "compound_origins.tsv",
            "comments.tsv",
            "chemical_data.tsv",
            "chebiId_inchi.tsv",
            "chebi_uniprot.tsv"
        }

        Dim Names As Tables.Names()
        Dim Accessions As Tables.Accession()
        Dim chemical_data As Tables.ChemicalData()
        Dim InChI As Tables.InChI()

        ''' <summary>
        ''' 从ChEBI的ftp文件夹之中加载数据 
        ''' </summary>
        ''' <param name="DIR"></param>
        Sub New(DIR As String)
            Call MyBase.New(DIR, {"*.tsv"})
            FileName = New FileNames(DIR)
        End Sub

        Public Function GetNames(Optional filename As String = "names.tsv") As Names()
            Return __lazyLoadData(Of Names)(Names, filename)
        End Function

        Public Function GetChemicalData(Optional filename As String = "chemical_data.tsv") As ChemicalData()
            Return __lazyLoadData(Of ChemicalData)(chemical_data, filename)
        End Function

        ''' <summary>
        ''' 加载化学结构式数据表格
        ''' </summary>
        ''' <param name="filename$"></param>
        ''' <returns></returns>
        Public Function GetInChI(Optional filename$ = "chebiId_inchi.tsv") As InChI()
            If InChI Is Nothing Then
                InChI = TsvFileIO.Load(Of InChI)(_DIR & "/" & filename, ).ToArray
            End If
            Return InChI
        End Function

        ''' <summary>
        ''' Lazy loading
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="fieldData"></param>
        ''' <param name="FileName"></param>
        ''' <returns></returns>
        Private Function __lazyLoadData(Of T As Tables.Entity)(ByRef fieldData As T(), FileName As String) As T()
            If fieldData.IsNullOrEmpty Then
                Dim path$ = $"{_DIR}/{FileName}"
                Dim st = Stopwatch.StartNew
                Call $"[{GetType(T).FullName}] Load data from {path.ToFileURL}".__DEBUG_ECHO
                fieldData = TsvFileIO.Load(Of T)(path).ToArray
                Call $"[LOAD_DATA_DONE] Performance load {fieldData.Length} objects in {st.ElapsedMilliseconds} ms...".__DEBUG_ECHO
            End If
            Return fieldData
        End Function

        Public Function GetDatabaseAccessions(Optional filename As String = "database_accession.tsv") As Accession()
            Return __lazyLoadData(Of Accession)(Accessions, filename)
        End Function
    End Class
End Namespace
