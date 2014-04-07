Feint Template Engine
==========

Template engine based on JADE.



Example template:

	html 
		head
			script(src="example_script.js")
		body
			#container
				h2 Best template engine ever
				|Friends:
				ul
					each friend in friends
						li {{friend}}

Rendered output :
with parrameter friends={"Piotr","Asia","Paweł","Mateusz"}

	<html>
	        <head>
	                <script src="example_script.js">
	                </script>
	        </head>
	        <body>
	                <div id="container">
	                        <h2>
	                                Best template engine ever
	                        </h2>
	                        Friends:
	                        <ul>
	                                <li>
	                                        Piotr
	                                </li>
	                                <li>
	                                        Asia
	                                </li>
	                                <li>
	                                        Pawel
	                                </li>
	                                <li>
	                                        Mateusz
	                                </li>
	                        </ul>
	                </div>
	        </body>
	</html>

Template engine supports inheritance:
(files must have fte extension)

Parrent:
	html
		head
			block head
		body
			#container
				block container
					|Container parent

Child:
	extends parrent_template

	block head
		script(src="example_script.js")

	block container
		h2.
			Best template engine ever
		|Friends:
		ul
			each friend in friends
				li {{friend}}

Rendered output :
with parrameter friends={"Piotr","Asia","Paweł","Mateusz"}
	<html>
	        <head>
	                <script src="example_script.js">
	                </script>
	        </head>
	        <body>
	                <div id="container">
	                		Container parent
	                        <h2>
	                                Best template engine ever
	                        </h2>
	                        Friends:
	                        <ul>
	                                <li>
	                                        Piotr
	                                </li>
	                                <li>
	                                        Asia
	                                </li>
	                                <li>
	                                        Pawel
	                                </li>
	                                <li>
	                                        Mateusz
	                                </li>
	                        </ul>
	                </div>
	        </body>
	</html>	


Example usage:
	TextReader reader = File.OpenText("child_template.fte");
	String text = reader.ReadToEnd();
	reader.Close();
	String[] friends={"Piotr","Asia","Paweł","Mateusz"};
	TemplateEngine templateEngine = new TemplateEngine(text, new { friends = friends });
	var rendered = templateEngine.Render();
