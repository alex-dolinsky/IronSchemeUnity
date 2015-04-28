using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using System.Reflection;

public class ClipboardHelper {
	private static PropertyInfo m_systemCopyBufferProperty = null;
	private static PropertyInfo GetSystemCopyBufferProperty() {
	    if (m_systemCopyBufferProperty == null) {
	        Type T = typeof (GUIUtility);
	        m_systemCopyBufferProperty = T.GetProperty ("systemCopyBuffer", BindingFlags.Static | BindingFlags.NonPublic);
	        if (m_systemCopyBufferProperty == null) { throw new Exception ("Can't access internal member 'GUIUtility.systemCopyBuffer' it may have been removed / renamed"); }
	    }
	    return m_systemCopyBufferProperty;
	}
	public static string clipBoard {
	    get { PropertyInfo P = GetSystemCopyBufferProperty ();
	          return (string) P.GetValue (null, null); }
	    set { PropertyInfo P = GetSystemCopyBufferProperty ();
	          P.SetValue (null, value, null); }
	}
}

public class SchemeReplWindow : EditorWindow {
	
	[MenuItem("Window/Repl")]
	public static void ShowSchemeReplWindow() {
		SchemeReplWindow window = EditorWindow.GetWindow<SchemeReplWindow>();
		window.title = "Repl";
	}
	
	// Repl
	List<string> replHistory = new List<string>();
	string currentForm = "";
	List<string> lineHistory = new List<string>();
	int lineHistoryIdx = 0;
	int lineHistorySize = 0;
	string currentLine = "";
	
	bool parensMatch(string form) {
		int lefts = form.Split('(').Length - 1;
		int rights = form.Split(')').Length - 1;
		return lefts == rights;
	}
	
	string Eval(string form) {
		try {
			var result = IronScheme.RuntimeExtensions.Eval(form);
			return result.ToString ();
		} catch (Exception e) {
			return e.ToString();
		}	
	}
	
	void OnEnable() {
		IronScheme.RuntimeExtensions.Eval("(import (ironscheme clr)) (clr-using UnityEngine)");
		IronScheme.RuntimeExtensions.Eval(String.Format("(library-path (cons \"{0}\" (library-path)))", Application.streamingAssetsPath));
	}
	
	Vector2 scroll_position = Vector2.zero;
	
	void OnGUI () {
		GUILayout.Label("Scheme Repl", EditorStyles.boldLabel);
		scroll_position = GUILayout.BeginScrollView(scroll_position, false, true);
		
		foreach (string line in replHistory) {
			GUILayout.Label(line);	
		}
		
		GUILayout.EndScrollView();
		
		GUI.SetNextControlName("repl-input");
		currentLine = GUILayout.TextField(currentLine);
		
		if (Event.current.isKey && Event.current.keyCode == KeyCode.Tab) {
			currentLine = ClipboardHelper.clipBoard;
		}

		if (Event.current.isKey && Event.current.keyCode == KeyCode.UpArrow) {
			lineHistoryIdx--;
			if (lineHistoryIdx >= 0 && lineHistoryIdx < lineHistorySize) {
				currentLine = lineHistory[lineHistoryIdx];
			} else {
				currentLine = "";
				lineHistoryIdx = lineHistorySize;
			}
		}

		if (Event.current.isKey && Event.current.keyCode == KeyCode.DownArrow) {
			lineHistoryIdx++;
			if (lineHistoryIdx >= 0 && lineHistoryIdx < lineHistorySize) {
				currentLine = lineHistory[lineHistoryIdx];
			} else {
				currentLine = "";
				lineHistoryIdx = -1;
			}
		}

		if (Event.current.isKey && Event.current.keyCode == KeyCode.Return) {
			replHistory.Add(currentLine);
			currentForm = currentForm + "\n" + currentLine;

			lineHistory.Add (currentLine);
			lineHistorySize++;
			lineHistoryIdx++;

			currentLine = "";
			if (currentForm != "" && parensMatch(currentForm)) {
				replHistory.Add(">" + Eval(currentForm));
				currentForm = "";
				lineHistoryIdx = lineHistorySize;
			}
			scroll_position.y = 100000;
		}
	}
	
	void OnInspectorUpdate() {
		// Redraw more often so we see results when they are evaluated.
		Repaint();
	}
	
	void OnFocus() {
		// Give keyboard focus to the repl textbox.
		GUI.FocusControl("repl-input");
	}
}
