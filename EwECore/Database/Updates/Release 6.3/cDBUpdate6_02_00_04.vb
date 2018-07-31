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
Imports EwEUtils.Database

''' --------------------------------------------------------------------------
''' <summary>
''' <para>Database update 6.2.0.04:</para>
''' <para>
''' <list type="bullet">
''' <item><description>Added multiple taxon codes.</description></item>
''' </list>
''' </para>
''' </summary>
''' --------------------------------------------------------------------------
Friend Class cDBUpdate6_02_00_04
    Inherits cDBUpdate

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateVersion"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateVersion() As Single
        Get
            Return 6.120004!
        End Get
    End Property

    ''' -----------------------------------------------------------------------
    ''' <inheritdocs cref="cDBUpdate.UpdateDescription"/>
    ''' -----------------------------------------------------------------------
    Public Overrides ReadOnly Property UpdateDescription() As String
        Get
            Return "Restructured taxonomy codes"
        End Get
    End Property

    Public Overrides Function ApplyUpdate(ByRef db As cEwEDatabase) As Boolean
        Dim bSucces As Boolean = db.Execute("ALTER TABLE EcopathTaxon ADD COLUMN CodeSAUP LONG")
        bSucces = bSucces And db.Execute("ALTER TABLE EcopathTaxon ADD COLUMN CodeFB LONG")
        bSucces = bSucces And db.Execute("ALTER TABLE EcopathTaxon ADD COLUMN CodeSLB LONG")
        bSucces = bSucces And db.Execute("ALTER TABLE EcopathTaxon ADD COLUMN CodeLCID TEXT(255)")
        bSucces = bSucces And db.Execute("ALTER TABLE EcopathTaxon ADD COLUMN CodeFAO TEXT(13)")
        db.Execute("ALTER TABLE EcopathTaxon DROP COLUMN CodeISCAAP, Code3A")
        Return bSucces
    End Function


End Class
