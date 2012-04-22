# [MarkPad](http://code52.org/DownmarkerWPF/)

By [code52](http://code52.org/) - Version 0.9


## Hotkeys

- `[F10]` - "Distraction free" mode, toggles the preview window on/off
- `[Ctrl++]` (or `[Ctrl =]`) - Zoom in
- `[Ctrl+-]` - Zoom out
- `[Ctrl+0]` - Reset zoom

Holding `Ctrl` and scrolling with the mouse wheel also zooms in and out.



## Contributors

If you want to contribute, get started at [MarkPad's GitHub page](https://github.com/Code52/DownmarkerWPF).

<div id="contributors"><em>Loading...</em></div>


## Components

- Autofac
- Awesomium
- Caliburn Micro
- AvalonEdit
- MahApps.Metro
- Ookii Dialogs
- XML-RPC.NET
- MarkdownDeep
- Notify Property Weaver






<script id="contributorTemplate" type="text/x-jQuery-tmpl">
{{each contributors}}
<img src="http://gravatar.com/avatar/${gravatar_id}?s=15" alt="${ name }" /> <a href="https://github.com/${login}">${name || login}</a> - (${contributions} commits)<br/>
{{/each}}
</script>

<script src="http://code.jquery.com/jquery.min.js" type="text/javascript"></script>
<script type="text/javascript" src="http://ajax.aspnetcdn.com/ajax/jquery.templates/beta1/jquery.tmpl.js"></script>
<script>
 $(function(){
    $.ajax({
        url: "http://github.com/api/v2/json/repos/show/Code52/DownmarkerWPF/contributors",
        dataType: 'jsonp',
        success: function(data) 
		{
			data.contributors = data.contributors.sort(function (a, b) 
			{ 
				if (a.contributions > b.contributions) return -1;
				if (a.contributions < b.contributions) return 1;
				return 0;
			});
			$('#contributors').html($("#contributorTemplate").tmpl(data));
        }
    });
  });
</script>