<h2>Description</h2>
<p>ActionStreetMap is an engine for building real city environment dynamically using OSM data.</p>
<img src="http://actionstreetmap.github.io/demo/images/Moscow_tower.png"/>
<p>The main goal is to simulate a real city, including following options:</p>
<ul>
<li>rendering of different models (e.g. buildings, roads, parks, rivers, etc.) using OSM data for given location and using terrain tiling approach (DONE partially).</li>
<li>rendering of non-flat world by using Data Elevation Model (DEM) files (DONE partially).</li>
<li>adding a character to the scene, which makes it capable to interact with the environment (DONE partially).</li>
<li>filling environment with people, cars, animals, which have realistic behaviour (Not started.)</li>
<li>using some external services to extend environment (e.g. weather, famous places, events, public transport schedule, etc.) (Not started).</li>
<li>Multiplayer (Not started).</li>
</ul>
<p>The engine can be used to build different 3D-games (like car simulations or GTA 2/3 ) or for some map tools on top of this framework (target is mobile devices). Actually, in this case the game world can be limited only by OSM maps which means that it will cover almost entire Earth. More info can be found <a href="http://actionstreetmap.github.io/demo/">here</a>.</p>

<p>Used technologies: Unity3D, C# (JavaScript is possible for Unity scripting in Demo app)</p>
			
<h2>Structure</h2>
<p>ActionStreetMap consists of two repositories:</p>
<ul>
	<li><a href="https://github.com/ActionStreetMap/framework">Framework</a> contains source code of ASM framework (Microsoft Visual Studio 2013 solution, .NET 3.5 as target platform, Unity is referenced via UnityEngine.dll assembly).</li>
	<li><a href="https://github.com/ActionStreetMap/demo">Demo</a> contains source code of showcase (Unity project, ASM framework is referenced via assemblies).</li>
</ul>

		
<h2>Software architecture</h2>
<p>ASM is built using Component Root and Dependency Injection patterns and consists of the following projects:</p>
<ul>
<li><b>ActionStreetMap.Infrastructure</b> - contains classes which helps to create framework.</li>
<li><b>ActionStreetMap.Core</b> contains classes of core map logic (e.g. MapCSS parser, Scene classes).</li>
<li><b>ActionStreetMap.Maps</b> contains OSM specific classes (e.g OSM parser, element visitors).</li>
<li><b>ActionStreetMap.Models</b> contains classes which builds unity game objects from real world abstractions.</li>
<li><b>ActionStreetMap.Explorer</b> contains application specific logic (e.g. MapCSS declaration rules, model builders).</li>
<li><b>ActionStreetMap.Tests</b> contains unit and integration tests. Also has Main function to run logic as console application (useful for profiling).</li>
</ul>

<h3>Development Tips</h3>
<ul>
	<li>Do not use Singleton pattern.</li>
	<li>Do not use DI as Service Locator.</li>
	<li>Follow GRASP and SOLID principles.</li>
	<li>Use TDD approach.</li>
</ul>