using UnityEngine;
using System;
using System.IO;
using System.Text;
using IronScheme;

using System.IO.Compression;
// subclass all scripts attached to game object from this
public abstract class Bootstrap : MonoBehaviour {
	static Bootstrap () {
		// bootstraps the scheme libs for all parent scripts only once
		var init_command = @"#!r6rs
							 (import (rnrs (6))
									 (ironscheme clr))
							 (clr-using UnityEngine)";
		IronScheme.RuntimeExtensions.Eval (init_command);
		var scheme_load_lib_command = (new StringBuilder ("(library-path (cons \"").Append (Application.streamingAssetsPath).Append ("\" (library-path)))")).ToString ();
		IronScheme.RuntimeExtensions.Eval (scheme_load_lib_command);
		IronScheme.RuntimeExtensions.Eval ("(import (macros))");
		IronScheme.RuntimeExtensions.Eval ("(import (unity))");
	}

	private DeflateStream DeflateString (this string str) {
		var stream = new MemoryStream ();
		var writer = new StreamWriter (stream);
		writer.Write (str);
		writer.Flush ();
		stream.Position = 0;
		return new DeflateStream (stream, CompressionMode.Compress, true);
	}

	protected void Start () {
		var scm_fname = this.GetType ().ToString ();  // has to be the same as CS fname and the child class name.
		var scm_script = (Resources.Load (scm_fname) as TextAsset).ToString ();
		// will create an isolated environment for every scheme script whilst retaining global bindings.
		scm_script = (new StringBuilder ("(let () (begin (define this {0}) ").Append (scm_script).Append ("))")).ToString ();
		IronScheme.RuntimeExtensions.Eval (scm_script, gameObject);
	}
}
