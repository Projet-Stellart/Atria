To get the TileMeshGeneration class:
	use the TileGenerationPlaceHolder.tscn scene and cast the TileMapGenerator Node3D to the TileMeshGeneration class.

To generate a random map:
	set the metadata parameters in godot scene editor
	use the GenerateGrid(int sizex, int sizey, Random rand) function with the sizex and sizey of the map and give the function a new Random instance -> Exemple: new Random()
	the GenerateGrid function return an int[,,] that can corresponds to int[heigh, x, y] x and y are permutable but heigh needs to be in the same place
	to get data from int[,,] -> tileTemplates(int[0, 0, 0]-1) the zeros can be changed to any value in the array
