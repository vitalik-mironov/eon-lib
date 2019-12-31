namespace Eon.Text {

	public readonly struct TextTemplateVar {

		public TextTemplateVar(string template, string span, int spanStart, int spanLength, string var) {
			Template = template;
			Span = span;
			SpanStart = spanStart;
			SpanLength = spanLength;
			Var = var;
		}

		public string Template { get; }

		public string Span { get; }

		public int SpanStart { get; }

		public int SpanLength { get; }

		public string Var { get; }

	}

}