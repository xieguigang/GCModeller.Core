﻿Imports LANS.SystemsBiology.Assembly
Imports LANS.SystemsBiology.Assembly.NCBI.CDD
Imports LANS.SystemsBiology.ComponentModel.Loci
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace ProteinModel

    Public Class DomainModel
        Implements sIdEnumerable, IKeyValuePairObject(Of String, Location)

        Public Property DomainId As String Implements sIdEnumerable.Identifier, IKeyValuePairObject(Of String, Location).Identifier
        Public Property Location As Location Implements IKeyValuePairObject(Of String, Location).Value

        Sub New(DomainId As String, Location As Location)
            Me.DomainId = DomainId
            Me.Location = Location
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("{0}: {1}", DomainId, Location.ToString)
        End Function
    End Class

    ''' <summary>
    ''' Domain identifier + Domain Location
    ''' </summary>
    Public Class DomainObject : Inherits SmpFile
        Implements sIdEnumerable

        Public Property Position As Location
        Public Property EValue As String
        Public Property BitScore As String
        ''' <summary>
        ''' 百分比位置
        ''' </summary>
        ''' <returns></returns>
        Public Property Location As Position

        Public Overrides Function ToString() As String
            Call Me.Position.Normalization()
            Return $"{Identifier}({Position.Left}|{Position.Right})"
        End Function

        ''' <summary>
        ''' 获取与本结构域相互作用的结构域的ID
        ''' </summary>
        ''' <param name="DOMINE"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetInteractionDomains(DOMINE As DOMINE.Database) As String()
            Dim Interactions = DOMINE.Interaction
            Dim LQuery = From Interaction As DOMINE.Tables.Interaction
                         In Interactions
                         Let DomainId As String = Interaction.GetInteractionDomain(MyBase.Identifier)
                         Where Not String.IsNullOrEmpty(DomainId)
                         Select DomainId '
            Return LQuery.ToArray
        End Function

        Sub New(SmpFile As SmpFile)
            MyBase.Identifier = SmpFile.Identifier
            MyBase.CommonName = SmpFile.CommonName
            MyBase.Describes = SmpFile.Describes
            MyBase.SequenceData = SmpFile.SequenceData
            MyBase.Id = SmpFile.Id
            MyBase.Title = SmpFile.Title
        End Sub

        Public Function CopyTo(Of T As DomainObject)() As T
            Dim Target As T = Activator.CreateInstance(Of T)()
            Target.Identifier = Identifier
            Target.BitScore = BitScore
            Target.CommonName = CommonName
            Target.Describes = Describes
            Target.EValue = EValue
            Target.Position = Position
            Target.SequenceData = SequenceData
            Target.Id = Id
            Target.Title = Title

            Return Target
        End Function

        Sub New()
        End Sub
    End Class
End Namespace