# World Shaper - A Tool For Shaping Larger Worlds

- External Dependencies: 
	- Demigiant: DoTween (Included in Plugins)
 	- Eflatun.SceneReference: `https://github.com/starikcetin/Eflatun.SceneReference.git#4.1.1`
- Internal Dependencies: 
 	- Puppeteer: `https://github.com/samuraininja84/Puppeteer`

# Editor Set-Up:
- To use World Shaper, you must first create an area handle. 
	- This scriptable object contains all the information about the area you are creating.
- You can create an area handle by right-clicking in the project window and selecting Create > World Shaper > New Area Handle.
	- Currently, for the system to work, you must have at least two area handles in the project, and all area handles must have "_Handle" at the end of their names.
	- This is a temporary limitation that will be removed in the future.
- Once you have created an area handle, you must assign a scene to it. 
	- This is the scene the player will be taken to when they enter the area.
- Next, you must create a connection. 
	- This sub-asset, scriptable-object, contains all the information about the connection between two areas.
	- You can create a connection by right-clicking on the area handle and selecting Create > World Shaper > New Connection.
	- You can also create a connection by pressing the "Add Connection" button in the Area Handle Inspector.
- Once you have created a connection, you must assign the connected scene to it. 
	- This is the area the player will be taken to when traveling through the connection.
- Next, you must choose the passage where the player will exit when they enter the area from the passage dropdown.
	- The connected area handle must have at least one connection for the dropdown to be populated.
- After you've done that for all the areas you want to connect, we can proceed to the next step.
  
# Scene Set-Up:
- World Shaper is dependent on the Transistor script.
	- Transistor is a script that handles the loading and unloading of scenes.
	- You can find a prefab for Transistor in the World Shaper / Resources / Prefabs folder.
	- You must have the Transistor script in your scene for the World Shaper system to work.
- For each scene you want a passage to connect, you must have game objects with the Passage script attached.
	- The Passage script is a component that contains information about the connection between two areas.
	- There is a prefab for passages that you can find in the World Shaper / Resources / Prefabs folder.
- Drag the prefab into the scene and position it where you want the player to exit when they enter the area.
- Assign the connected area handle to the passage and set the end passage to the passage you want the player to come out of after then exit the area.
  
# Notes:
- The World Shaper system is designed to be as flexible as possible.
	- You can create as many areas and connections as you want.
	- You can create as many passages as you want in each scene.
	- You can create as many connections as you want between areas.
- If you wish to load a scene via code, use the ChangeArea method in Transistor.
	- This method takes in the name of the area you want to load (and if you wish, the passage you want the player to exit).
- If you wish to reload a scene via code, use the ReloadCurrentArea method in Transistor.
- The Can Interact bool on Passages determines if the player can interact with the passage.
	- If true, the player can interact with the passage and travel to the connected area.
	- If false, the player will not be able to interact with the passage.
	- It prevents the player from interacting with the passage immediately after loading into the scene.
- If you wish to have one-way passages, you can type enum on a Passage to Closed; that way, you can only enter an area through that Passage but not leave the area from it.
