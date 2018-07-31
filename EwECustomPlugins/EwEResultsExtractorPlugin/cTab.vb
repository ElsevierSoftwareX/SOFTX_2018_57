' ===============================================================================
' This file is part of Ecopath with Ecosim (EwE)
'
' EwE is free software: you can redistribute it and/or modify it under the terms
' of the GNU General Public License version 2 as published by the Free Software 
' Foundation.
'
' EwE is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
' without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR 
' PURPOSE. See the GNU General Public License for more details.
'
' You should have received a copy of the GNU General Public License along with EwE.
' If not, see <http://www.gnu.org/licenses/gpl-2.0.html>. 
'
' Copyright 1991- 
'    UBC Institute for the Oceans and Fisheries, Vancouver BC, Canada, and
'    Ecopath International Initiative, Barcelona, Spain
' ===============================================================================
'

'Option Strict On

'Public Class cTab
'    Private Sub 
'End Class


'    Friend m_TabName As String
'    Friend m_Header As String
'    Friend m_ColTitles As List(Of cColumnHeader)
'    Private m_ContainsSubTitles As Boolean
'    Friend m_RowNames() As String
'    Friend m_BodyData() As Single
'    Friend m_ColDataAdditional() As Single
'    Friend m_RowDataAdditional() As Single

'    Public Class cColumnHeader
'        Private m_SuperTitle As String
'        Private m_SubTitles As List(Of String)

'        Public Sub New(ByVal SuperTitle As String, ByVal SubTitles As List(Of String))
'            m_SuperTitle = SuperTitle
'            m_SubTitles = SubTitles
'        End Sub

'        Public ReadOnly Property GetSuperTitle() As String
'            Get
'                Return m_SuperTitle
'            End Get
'        End Property

'        Public ReadOnly Property GetSubTitles() As List(Of String)
'            Get
'                Return m_SubTitles
'            End Get
'        End Property

'    End Class

'    Public Sub New(ByVal TabName As String, ByVal Header As String, _
'                ByVal ColTitles As List(Of cColumnHeader), _
'                ByVal RowNames() As String, ByVal BodyData() As Single, _
'                      Optional ByVal ColDataAdditional() As Single = Nothing, _
'                      Optional ByVal RowDataAdditional() As Single = Nothing)

'        m_TabName = TabName
'        m_Header = Header
'        m_ColTitles = ColTitles
'        m_ContainsSubTitles = ContainsSubTitles
'        m_RowNames = RowNames
'        m_BodyData = BodyData
'        m_ColDataAdditional = ColDataAdditional
'        m_RowDataAdditional = RowDataAdditional

'    End Sub

'    Public Sub New(ByVal TabName As String, ByVal Header As String, _
'            ByVal ColTitles As List(Of String), _
'            ByVal ContainsSubTitles As Boolean, _
'            ByVal RowNames() As String, ByVal BodyData() As Single, _
'                  Optional ByVal ColDataAdditional() As Single = Nothing, _
'                  Optional ByVal RowDataAdditional() As Single = Nothing)

'        m_TabName = TabName
'        m_Header = Header
'        m_ColTitles = ColTitles
'        m_ContainsSubTitles = ContainsSubTitles
'        m_RowNames = RowNames
'        m_BodyData = BodyData
'        m_ColDataAdditional = ColDataAdditional
'        m_RowDataAdditional = RowDataAdditional

'    End Sub

'    Public Sub AddHeader(ByVal SuperTitle As String, Optional ByVal SubTitle As List(Of String) = Nothing)
'        m_ColTitles.Add(New cColumnHeader(SuperTitle, SubTitle))
'    End Sub



'End Class
