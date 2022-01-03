using System;
using System.Text.Json.Serialization;

namespace {{rootnamespace}}
{
	public enum {{className | safe_name | pascal_case}}
	{
	{%- for member in properties -%}
	{%- if member.summary -%}
		/// <summary>{{member.summary}}</summary>
	{%- endif -%}
		{{member.name | safe_name | pascal_case}} = {{member.value}},
	{%- endfor -%}
	}
}