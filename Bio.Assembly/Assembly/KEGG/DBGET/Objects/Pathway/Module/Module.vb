﻿#Region "Microsoft.VisualBasic::d369f72500c78e5ed062c9cbcba53c78, ..\GCModeller\core\Bio.Assembly\Assembly\KEGG\DBGET\Objects\Pathway\Module\Module.vb"

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

Imports System.Text.RegularExpressions
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text.HtmlParser
Imports SMRUCC.genomics.Assembly.KEGG.WebServices.InternalWebFormParsers

Namespace Assembly.KEGG.DBGET.bGetObject

    ''' <summary>
    ''' KEGG MODULE is a collection of manually defined functional units, called KEGG modules and identified by the M numbers, 
    ''' used for annotation and biological interpretation of sequenced genomes. 
    ''' 
    ''' There are four types of KEGG modules:
    ''' pathway modules – representing tight functional units In KEGG metabolic pathway maps, such As M00002 (Glycolysis, core Module involving three-carbon compounds)
    ''' structural complexes – often forming molecular machineries, such As M00072 (Oligosaccharyltransferase)
    ''' functional sets – For other types Of essential sets, such As M00360 (Aminoacyl-tRNA synthases, prokaryotes)
    ''' signature modules – As markers Of phenotypes, such As M00363 (EHEC pathogenicity signature, Shiga toxin)
    ''' 
    ''' The entire list Of KEGG modules can be viewed from the BRITE hierarchy file:
    ''' KEGG modules
    ''' 
    ''' Logical Expression
    ''' 
    ''' The M number entry Is defined by a logical expression Of K numbers (And other M numbers), allowing automatic evaluation Of whether the gene Set Is complete, i.e., the Module Is present, In a given genome. A space Or a plus sign represents an And operation, And a comma sign represents an Or operation In this expression. A plus sign Is used For a molecular complex And a minus sign designates an Optional item In the complex. 
    ''' 
    ''' Each space-separated unit Is called a block, And the distinction Is made for:
    ''' complete modules
    ''' incomplete but almost complete modules With only 1 Or 2 blocks missing
    ''' all modules that contain any matching K numbers
    ''' in the KEGG MODULE database, as well as in the KEGG Mapper tool "Reconstruct Module".
    ''' Module Map
    ''' 
    ''' KEGG modules are associated With graphical diagrams called Module maps. For example, M00002 represents glycolysis core Module involving three-carbon compounds And its organism specific Module can be selected from the pop-up menu Or directly specified In the form Of hsa_M00002. While KEGG pathway maps are all manually drawn, KEGG Module maps are computationally generated from the text definition describe above. There Is also a difference Of how organism-specific versions are generated. Organism specific Module entries are created only For complete modules, While organism-specific pathway map entries are created When a few matching elements exist under a predefined taxonomic constraint.
    ''' Ortholog Table
    ''' 
    ''' The ortholog table Is a useful tool To check completeness And consistency Of genome annotations. It shows currently annotated genes In individual genomes For a given Set Of K numbers, together With coloring Of adjacent genes (operon-Like structures) On the chromosome. Each KEGG Module contains a link To the corresponding ortholog table, such As For M00165, together With Option To Select complete Or other modules.
    ''' Taxonomy Mapping
    ''' 
    ''' Each KEGG module also contains a link to KEGG taxonomy mapping, showing which organisms Or organism groups have the module under the taxonomic classification of KEGG organisms. The taxonomy link from the ortholog table (designated by T) allows mapping of both complete And incomplete modules. The result Is shown in the color coding shown below.
    '''  	complete
    '''  	incomplete, 1 block missing
    '''  	incomplete, 2 blocks missing
    ''' KEGG Modules And Reaction Modules
    ''' 
    ''' It Is interesting to note that KEGG modules sometimes correspond to reaction modules extracted from purely chemical properties as summarized in the following BRITE hierarchy file:
    ''' KEGG reaction modules
    ''' A New category of KEGG metabolic pathway maps, called overview maps, shows this correspondence as well as an overall architecture of the metabolic network. The following Is an example taken from the overview map for Degradation of aromatic compounds. 
    ''' 
    ''' A single M number Or a combination of M numbers can be used for characterizing phenotypic features encoded in the genome. For example, the BTX (benzene, toluene, And xylene) degradation capacity can be seen from the following diagram where M numbers are linked to the ortholog tables indicating which organisms have complete modules.
    ''' benzene	— M00548 →	catechol
    ''' toluene	— M00538 →	benzoate	— M00551 →	catechol	— M00569 →	meta-cleavage
    ''' — M00568 →	ortho-cleavage
    ''' xylene	— M00537 →	methyl-
    ''' benzoate	— M00551 →	methyl-
    ''' catechol	— M00569 →	meta-cleavage
    ''' — M00568 →	ortho-cleavage
    ''' This example can be rewritten In terms Of the reaction modules.
    ''' benzene	— RM006 →	catechol
    ''' toluene	— RM003 →	benzoate	— RM005 →	catechol	— RM009 →	meta-cleavage
    ''' — RM008 →	ortho-cleavage
    ''' xylene	— RM003 →	methyl-
    ''' benzoate	— RM005 →	methyl-
    ''' catechol	— RM009 →	meta-cleavage
    ''' — RM008 →	ortho-cleavage
    ''' See more details In:
    ''' Muto, A., Kotera, M., Tokimatsu, T., Nakagawa, Z., Goto, S., And Kanehisa, M.; Modular architecture of metabolic pathways revealed by conserved sequences of reactions. J. Chem. Inf. Model. 53, 613-622 (2013). [pubmed] [pdf] 
    ''' 
    ''' Kanehisa, M.; Chemical And genomic evolution of enzyme-catalyzed reaction networks. FEBS Lett. 587, 2731-2737 (2013). [pubmed] [pdf] 
    ''' 
    ''' Kanehisa, M., Goto, S., Sato, Y., Kawashima, M., Furumichi, M., And Tanabe, M.; Data, information, knowledge And principle back to metabolism in KEGG. Nucleic Acids Res. 42, D199–D205 (2014). [pubmed] [pdf]
    ''' 
    ''' </summary>
    <XmlRoot("KEGG.MODULE", Namespace:="http://www.genome.jp/dbget-bin/get_linkdb?-t+module+genome")>
    Public Class [Module] : Inherits ComponentModel.PathwayBrief

        Public Property Name As String
        Public Property Pathway As KeyValuePair()
        Public Property Compound As KeyValuePair()
        Public Property Reaction As KeyValuePair()
            Get
                Return _lstReactions
            End Get
            Set(value As KeyValuePair())
                _lstReactions = value
                _dictReaction = KeyValuePair.ToDictionary(value)
            End Set
        End Property

        Public Property PathwayGenes As KeyValuePair()

        Dim _lstReactions As KeyValuePair()
        Dim _dictReaction As Dictionary(Of String, String)

        Public Function ContainsReaction(Id As String) As Boolean
            If _dictReaction.IsNullOrEmpty Then
                Return False
            End If
            Return _dictReaction.ContainsKey(Id)
        End Function

        Public Overrides ReadOnly Property BriteId As String
            Get
                Return EntryId.Split("_"c).Last.ToUpper
            End Get
        End Property

        Default Public ReadOnly Property ReactionItem(Entry As String) As String
            Get
                If _dictReaction.IsNullOrEmpty Then
                    Return ""
                End If

                If Not _dictReaction.ContainsKey(Entry) Then
                    Return ""
                Else
                    Dim str As String = _dictReaction(Entry)
                    If String.IsNullOrEmpty(str) Then
                        Return "-"
                    Else
                        Return str
                    End If
                End If
            End Get
        End Property

        Public Shared Function GetKEGGReactionIdlist(mods As IEnumerable(Of [Module])) As String()
            Dim buf As New List(Of String)

            For Each [mod] As [Module] In mods
                If [mod].Reaction.IsNullOrEmpty Then
                    Continue For
                End If

                buf += From rxn As KeyValuePair
                       In [mod].Reaction
                       Select rxn.Key
            Next

            Return (From strId As String In buf Select strId Distinct).ToArray
        End Function

        Public Shared Function Download(url As String) As [Module]
            Dim WebForm As New WebForm(url)

            If WebForm.Count = 0 Then
                Return Nothing
            End If

            Dim SpeciesCode As String = WebForm.GetValue("Organism").FirstOrDefault
            Dim [Module] As [Module] = New [Module]
            SpeciesCode = Regex.Match(SpeciesCode, "\[GN:<a href="".+?"">.+?</a>]").Value.GetValue

            [Module].EntryId = Regex.Match(WebForm.GetValue("Entry").FirstOrDefault, "[a-z]+_M\d+").Value
            [Module].Name = WebForm.GetValue("Name").FirstOrDefault
            [Module].Description = WebForm.GetValue("Definition").FirstOrDefault
            [Module].Pathway = KEGG.WebServices.InternalWebFormParsers.WebForm.parseList(WebForm.GetValue("Pathway").FirstOrDefault, "<a href=""/kegg-bin/show_pathway\?.+?"">.+?</a>")
            [Module].PathwayGenes = KEGG.WebServices.InternalWebFormParsers.WebForm.parseList(WebForm.GetValue("Gene").FirstOrDefault, String.Format("<a href=""/dbget-bin/www_bget\?{0}:.+?"">.+?</a>", SpeciesCode))
            [Module].Compound = KEGG.WebServices.InternalWebFormParsers.WebForm.parseList(WebForm.GetValue("Compound").FirstOrDefault, "<a href=""/dbget-bin/www_bget\?cpd:.+?"">.+?</a>")
            [Module].Reaction = KEGG.WebServices.InternalWebFormParsers.WebForm.parseList(WebForm.GetValue("Reaction").FirstOrDefault, "<a href=""/dbget-bin/www_bget\?rn:.+?"">.+?</a>")

            Return [Module]
        End Function

        ''' <summary>
        ''' 得到当前的模块之中的基因的编号的列表，这是个安全的函数，不会返回空值
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function GetPathwayGenes() As String()
            If PathwayGenes.IsNullOrEmpty Then
                Return New String() {}
            End If

            Return PathwayGenes.ToArray(Function(x) x.key)
        End Function
    End Class
End Namespace
