#if GEN || GEN_HTML
using System;
using System.Collections.Generic;
#endif
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.Extensions.Configuration;
using NeKzBot.Data;

namespace NeKzBot.Services
{
	public enum CvarGameType
	{
		HalfLife2,
		Portal,
		Portal2
	}

	public class SourceCvarService
	{
		// Cache data to make things faster
		private ConcurrentDictionary<string, SourceCvarData> _hl2Cache;
		private ConcurrentDictionary<string, SourceCvarData> _p1Cache;
		private ConcurrentDictionary<string, SourceCvarData> _p2Cache;

		private readonly IConfiguration _config;
		private readonly LiteDatabase _dataBase;

		public SourceCvarService(IConfiguration config, LiteDatabase dataBase)
		{
			_config = config;
			_dataBase = dataBase;
		}

		public Task Initialize()
		{
#if GEN
			_dataBase.DropCollection(nameof(SourceCvarService));
			_ = Gen("private/resources/hl2-cvars", CvarGameType.HalfLife2);
			_ = Gen("private/resources/p1-cvars", CvarGameType.Portal);
			_ = Gen("private/resources/p2-cvars", CvarGameType.Portal2);
#endif
			_hl2Cache = new ConcurrentDictionary<string, SourceCvarData>();
			_p1Cache = new ConcurrentDictionary<string, SourceCvarData>();
			_p2Cache = new ConcurrentDictionary<string, SourceCvarData>();

			var db = _dataBase
				.GetCollection<SourceCvarData>(nameof(SourceCvarService));

			foreach (var data in db.Find(d => d.Type == CvarGameType.HalfLife2))
				_hl2Cache.TryAdd(data.Name, data);
			foreach (var data in db.Find(d => d.Type == CvarGameType.Portal))
				_p1Cache.TryAdd(data.Name, data);
			foreach (var data in db.Find(d => d.Type == CvarGameType.Portal2))
				_p2Cache.TryAdd(data.Name, data);

#if GEN_HTML
			_ = GenHtml("hl2.html", "Half-Life 2", "HL2", CvarGameType.HalfLife2);
			_ = GenHtml("p1.html", "Portal", "P1", CvarGameType.Portal);
			_ = GenHtml("p2.html", "Portal 2", "P2", CvarGameType.Portal2);
#endif
			return Task.CompletedTask;
		}

		public Task<SourceCvarData> LookUpCvar(string cvar, CvarGameType type)
		{
			var result = default(SourceCvarData);
			switch (type)
			{
				case CvarGameType.HalfLife2:
					_hl2Cache.TryGetValue(cvar, out result);
					break;
				case CvarGameType.Portal:
					_p1Cache.TryGetValue(cvar, out result);
					break;
				case CvarGameType.Portal2:
					_p2Cache.TryGetValue(cvar, out result);
					break;
			}
			return Task.FromResult(result);
		}
#if GEN
		private Task Gen(string file, CvarGameType type)
		{
			var db = _dataBase.GetCollection<SourceCvarData>(nameof(SourceCvarService));
			var data = new List<SourceCvarData>();

			var dev = new List<string>();
			var hidden = new List<string>();

			using (var fs = System.IO.File.OpenRead(file +  ".txt"))
			using (var sr = new System.IO.StreamReader(fs))
			{
				while (!sr.EndOfStream)
				{
					var line = sr.ReadLine();
					if (line == "[cvar_list]") break;

					var values = line.Split(' ');
					var cvar = values[0];
					var flag = values[1];

					Console.WriteLine(cvar);

					if (flag == "[cvar_dev_hidden]")
					{
						dev.Add(cvar);
						hidden.Add(cvar);
					}
					else if (flag == "[cvar_dev]")
						dev.Add(cvar);
					else if (flag == "[cvar_hidden]")
						hidden.Add(cvar);
					else
						throw new Exception("Invalid cvar flag!");
				}

				var text = sr.ReadToEnd();
				foreach (var cvar in text.Split(new string[] { "[end_of_cvar]" }, StringSplitOptions.RemoveEmptyEntries))
				{
					var values = cvar.Split(new string[] { "[cvar_data]" }, StringSplitOptions.None);
					if (values.Length != 4)
						throw new Exception("Invalid cvar data!");

					var name = values[0].Trim();
					Console.WriteLine($"Name: {name}");

					var defaultvalue = values[1].Trim();
					Console.WriteLine($"Default: {defaultvalue}");

					var flags = values[2].Trim().Split(',').ToList();
					if (flags[0] == string.Empty)
						flags.RemoveAt(0);

					for (int i = 0; i < flags.Count; i++)
						flags[i] = flags[i].Replace("\"", string.Empty).Trim();

					if (dev.Contains(name))
						flags.Add("dev");
					else if (hidden.Contains(name))
						flags.Add("hidden");
					Console.WriteLine($"Flags: {string.Join("/", flags)}");

					var description = values[3].Trim();
					Console.WriteLine($"Description: {description}");
					Console.WriteLine("---------------------------------------------------");

					data.Add(new SourceCvarData()
					{
						Type = type,
						Name = name,
						DefaultValue = defaultvalue,
						Flags = flags.AsEnumerable(),
						HelpText = description
					});
				}
			}

			db.Upsert(data);
			return Task.CompletedTask;
		}
#endif
#if GEN_HTML
		internal Task GenHtml(string file, string title, string shortTitle, CvarGameType type)
		{
			var cache = default(IEnumerable<SourceCvarData>);
			if (type == CvarGameType.HalfLife2)
				cache = _hl2Cache.Values.OrderBy(v => v.Name);
			else if (type == CvarGameType.Portal)
				cache = _p1Cache.Values.OrderBy(v => v.Name);
			else if (type == CvarGameType.Portal2)
				cache = _p2Cache.Values.OrderBy(v => v.Name);

			if (System.IO.File.Exists("cvars/" + file))
				System.IO.File.Delete("cvars/" + file);

			using (var fs = System.IO.File.OpenWrite("cvars/" + file))
			using (var sw = new System.IO.StreamWriter(fs))
			{
				sw.WriteLine(
$@"<!DOCTYPE html>
<html>
	<head>
		<title>{title} Cvars | nekzor.github.io</title>
		<link href=""https://fonts.googleapis.com/css?family=Roboto"" rel=""stylesheet"">
		<link href=""https://fonts.googleapis.com/icon?family=Material+Icons"" rel=""stylesheet"">
		<link href=""https://cdnjs.cloudflare.com/ajax/libs/materialize/1.0.0-alpha.4/css/materialize.min.css"" rel=""stylesheet"">
	</head>
	<body class=""white-text blue-grey darken-4"">
		<nav class=""nav-extended blue-grey darken-3"">
			<div class=""nav-wrapper"">
				<div class=""col s12 hide-on-small-only"">
					<a href=""../index.html"" class=""breadcrumb"">&nbsp;&nbsp;&nbsp;nekzor.github.io</a>
					<a href=""{file}"" class=""breadcrumb"">{title} Cvars</a>
				</div>
				<div class=""col s12 hide-on-med-and-up"">
					<a href=""#"" data-target=""slide-out"" class=""sidenav-trigger""><i class=""material-icons"">menu</i></a>
					<a href=""{file}"" class=""brand-logo center"">{shortTitle}</a>
				</div>
			</div>
		</nav>
		<ul id=""slide-out"" class=""sidenav hide-on-med-and-up"">
			<li><a href=""../index.html"">nekzor.github.io</a></li>
			<li><a href=""{file}"">{title} Cvars</a></li>
		</ul>
		<div id=""cvars"">
			<div class=""row""></div>
			<div class=""row"">
				<div class=""col s12 m12 l2"">
					<input id=""search-box"" class=""search white-text"" placeholder=""Search"" />
				</div>
				<div class=""col s12 m12 l1"">
					<br>
					<label>
						<input id=""cbx-name"" type=""checkbox"" checked=""checked"" onclick=""updateFilter()"" />
						<span>Name</span>
					</label>
				</div>
				<div class=""col s12 m12 l1"">
					<br>
					<label>
						<input id=""cbx-default"" type=""checkbox"" checked=""checked"" onclick=""updateFilter()"" />
						<span>Default</span>
					</label>
				</div>
				<div class=""col s12 m12 l1"">
					<br>
					<label>
						<input id=""cbx-flags"" type=""checkbox"" checked=""checked"" onclick=""updateFilter()"" />
						<span>Flags</span>
					</label>
				</div>
				<div class=""col s12 m12 l1"">
					<br>
					<label>
						<input id=""cbx-help-text"" type=""checkbox"" checked=""checked"" onclick=""updateFilter()"" />
						<span>Help Text</span>
					</label>
				</div>
			</div>
			<div class=""row"">
				<div class=""col s12"">
					<table class=""highlight"">
						<thead>
							<tr>
								<th>Name</th>
								<th>Default</th>
								<th>Flags</th>
								<th>Help Text</th>
							</tr>
						</thead>
						<tbody class=""list"">");
				foreach (var cvar in cache)
				{
					var flags = (cvar.Flags.Any())
						? string.Join("/",cvar.Flags)
						: "-";
					var description = (!string.IsNullOrEmpty(cvar.HelpText))
						? cvar.HelpText
							.Replace('\n', ' ')
							.Replace('\t', ' ')
							.Replace('\r', ' ')
							.Replace("\"", "&quot;")
							.Replace("'", "	&apos;")
							.Replace("<", "	&lt;")
							.Replace(">", "	&gt;")
						: "-";

					sw.WriteLine(
$@"							<tr>
								<td class=""name"">{cvar.Name}</td>
								<td class=""default"">{cvar.DefaultValue}</td>
								<td class=""flags"">{flags}</td>
								<td class=""help-text"">{description}</td>
							</tr>");
				}

				sw.WriteLine(
$@"						</tbody>
					</table>
				</div>
			</div>
		</div>
		<script src=""https://code.jquery.com/jquery-3.3.1.min.js"" integrity=""sha256-FgpCb/KJQlLNfOu91ta32o/NMZxltwRo8QtmkMRdAu8="" crossorigin=""anonymous""></script>
		<script src=""https://cdnjs.cloudflare.com/ajax/libs/materialize/1.0.0-alpha.4/js/materialize.min.js""></script>
		<script>
			$(document).ready(function() {{
				$('.sidenav').sidenav();
			}});
		</script>
		<script src=""https://cdnjs.cloudflare.com/ajax/libs/list.js/1.5.0/list.min.js""></script>
		<script>
			var cvars = new List('cvars', {{ valueNames: [ 'name', 'default', 'flags', 'help-text' ] }});

			function updateFilter() {{
				var filter = [];
				if (document.getElementById(""cbx-name"").checked) filter.push(""name"");
				if (document.getElementById(""cbx-default"").checked) filter.push(""default"");
				if (document.getElementById(""cbx-flags"").checked) filter.push(""flags"");
				if (document.getElementById(""cbx-help-text"").checked) filter.push(""help-text"");
				cvars.valueNames = filter;
				cvars.search();
				cvars.reIndex();
				cvars.search(document.getElementById(""search-box"").value);
			}}
		</script>
	</body>
</html>");
			}
			return Task.CompletedTask;
		}
#endif
	}
}