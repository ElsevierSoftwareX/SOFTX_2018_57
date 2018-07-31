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

Option Strict On

Imports EwEPlugin
Imports EwEUtils.Database
Imports System.Data

''' --------------------------------------------------------------------------
''' <summary>
''' <para>Database update 6.0.4.001:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Fixed stanza life stage vbK issue.</description></item>
''' <item><description>Added Ecopath Area.</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_00_04_0001
    Inherits cDBUpdate

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' The actual update logic.
    ''' </summary>
    ''' <param name="db">Database to modify.</param>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Public Overrides Function ApplyUpdate(ByRef db As EwEUtils.Database.cEwEDatabase) As Boolean

        Dim reader As IDataReader = Nothing
        Dim bSucces As Boolean = True

        ' Move vbK from EcopathGroup to StanzaLifeStage
        bSucces = bSucces And db.Execute("ALTER TABLE StanzaLifeStage ADD COLUMN vbK SINGLE")
        ' Access AARGH moment:
        ' - Nested SET queries do not work 
        'bSucces = bSucces And db.Execute("UPDATE StanzaLifeStage SET vbK=EcopathGroup.vbK FROM EcopathGroup WHERE EcopathGroup.GroupID=StanzaLifeStage.GroupID")
        ' - Updates using INNER JOIN will lock tables temporarily, causing the following ALTER TABLE command to fail
        'bSucces = bSucces And db.Execute("UPDATE StanzaLifeStage INNER JOIN EcopathGroup ON EcopathGroup.GroupID=StanzaLifeStage.GroupID SET StanzaLifeStage.vbK=EcopathGroup.vbK")
        ' - This also does not work (throws "Operation must be an updatable query")
        'bSucces = bSucces And db.Execute("UPDATE StanzaLifeStage SET vbK= (SELECT EcopathGroup.vbK FROM EcopathGroup WHERE EcopathGroup.GroupID=StanzaLifeStage.GroupID)")

        ' *DEEP sigh*
        reader = db.GetReader("SELECT EcopathGroup.GroupID, EcopathGroup.vbK FROM EcopathGroup, StanzaLifeStage WHERE EcopathGroup.GroupID=StanzaLifeStage.GroupID")
        If reader IsNot Nothing Then
            While reader.Read
                bSucces = bSucces And db.Execute(String.Format("UPDATE StanzaLifeStage SET vbK={0} WHERE GroupID={1}", reader("vbK"), reader("GroupID")))
            End While
        End If
        db.ReleaseReader(reader)
        ' Now drop the vbK column from GroupInfo
        bSucces = bSucces And db.Execute("ALTER TABLE EcopathGroup DROP COLUMN vbK")

        Return bSucces

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' This method provides the text that will be entered in the update log in
    ''' the database.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateDescription() As String
        Get
            Return "Fixes stanza life stage vbK issue." + Environment.NewLine + "Added Ecopath Area"
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' This method provides the update version number that will be entered in
    ''' the update log of the database. This version number is also used to check
    ''' whether an update should run.
    ''' </summary>
    ''' <remarks>
    ''' If <see cref="cCore.NULL_VALUE">cCore.NULL_VALUE</see> is provided, the
    ''' update is ran regardless of version number.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateVersion() As Single
        Get
            Return 6.04001!
        End Get
    End Property

End Class
