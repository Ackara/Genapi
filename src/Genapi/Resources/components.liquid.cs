using System;
using System.Text.Json.Serialization;

namespace {{rootnamespace}}
{
	public class {{className}}
	{
	{%- for member in properties -%}
	{%- if member.summary -%}
		/// <summary>{{member.summary}}</summary>
	{%- endif -%}
		[JsonPropertyName("{{member.name}}")]
		public {{member.type}} {{member.name | safe_name | pascal_case}} {{ '{' }} get; set; {{ '}' }}
	
	{%- endfor -%}
	}
}