<h2>Description</h2>
<p>ActionStreetMap (ASM) is an engine for building real city environment dynamically using OSM data.</p>
<img src="http://actionstreetmap.github.io/demo/images/Moscow_tower.png"/>
<p>The main goal is to simulate a real city, including the following:</p>
<ul>
<li>rendering of different models (e.g. buildings, roads, parks, rivers, etc.) using OSM data for given location and using terrain tiling approach.</li>
<li>rendering of non-flat world by using Data Elevation Model (DEM) files.</li>
<li>adding a character to the scene, which makes it capable to interact with the environment.</li>
<li>filling environment with people, cars, animals, which have realistic behaviour</li>
<li>using some external services to extend environment (e.g. weather, famous places, events, public transport schedule, etc.).</li>
<li>Multiplayer.</li>
</ul>
<p>The engine can be used to build different 3D-games (like car simulations or GTA 2/3 ) or for some map tools on top of this framework (target is mobile devices). Actually, in this case the game world can be limited only by OSM maps which means that it will cover almost entire Earth. More info can be found <a href="http://actionstreetmap.github.io/demo/">here</a>.</p>
<p>Internally, framework is decoupled from Unity3D as much as it's possible taking into account related performance impact. In theory, it can be ported to other platforms to build map data visualization application.</p>

<p>Used technologies: Unity3D, C# (JavaScript is possible for Unity scripting in Demo app), Reactive Extensions</p>
			
<h2>Structure</h2>
<p>ActionStreetMap consists of two repositories:</p>
<ul>
	<li><a href="https://github.com/ActionStreetMap/framework">Framework</a> contains source code of ASM framework (Microsoft Visual Studio 2013 solution, .NET 3.5 as target platform, Unity is referenced via UnityEngine/ UnityEditor assemblies).</li>
	<li><a href="https://github.com/ActionStreetMap/demo">Demo</a> contains source code of showcase (Unity project, ASM framework is referenced via assemblies).</li>
</ul>

		
<h2>Software architecture</h2>
<p>ASM is built using Composition Root and Dependency Injection patterns and consists of the following projects:</p>
<ul>
<li><b>ActionStreetMap.Infrastructure</b> - contains classes which helps to build frameworks infrastructure (e.g. dependency injection container, file system API, etc.).</li>
<li><b>ActionStreetMap.Core</b> contains classes of core map logic (e.g. map primitives, MapCSS parser and rules, scene classes).</li>
<li><b>ActionStreetMap.Maps</b> contains OSM specific classes (e.g map data index, OSM data parser, element visitors).</li>
<li><b>ActionStreetMap.Explorer</b> contains application specific logic (e.g. MapCSS declaration rules, model builders, has unity classes dependency).</li>
<li><b>ActionStreetMap.Unity</b> contains platform specific code (uses conditional compilation symbols to implement platform specific features).</li>
<li><b>ActionStreetMap.Tests</b> contains unit and integration tests. Also has Main function to run logic as console application (useful for profiling, debugging map specific code).</li>
</ul>

<h3>Current status</h3>
<p>Project is under development. Rendering of OSM data can be considered as done with some major issues.</p>