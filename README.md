Feint Template Engine
==========

Template engine based on JADE.



example parsed template

	html test
		test#foo.bar
			test2#d.a(abc="a")
				textBlock.
					aaaa
					asdsdad
					asdasda
					asdasdasdasdasd
					asdasda
				if true
					testBlock
						each val, index in collection
							|abcd {{val}}::{{index}}
