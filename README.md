# Project-2-COMP30019
Project 2 for COMP30019 at University of Melbourne. 3D platformer.

README
--
SOURCES:

Textures generated using http://cpetry.github.io/TextureGenerator-Online/
Normal maps generated using http://cpetry.github.io/NormalMap-Online/
Wood texture from http://www.techcredo.com/wp-content/uploads/2010/06/plank-wooden-texture.jpg and http://graphicdesignjunction.com/wp-content/uploads/2013/03/wood-textures-high-quality-2.jpg
Smoke texture from https://www.filterforge.com/filters/6252-bump.jpg
Door texture from http://www.homedepot.com/catalog/productImages/1000/cb/cb56bbbc-6bcd-4033-9b2a-5ffcbf7513e5_1000.jpg
Some code borrowed from http://stackoverflow.com/questions/35422692/how-to-make-an-object-to-glow-in-unity3d

The application is a 3D platforming game. The game teaches its controls to the user; the touch screen, Z, X, and spacebar keys are used to control various aspects.
Objects are largely built-in Unity meshes. The chest mesh was created in SketchUp, and the pyramid was created in Blender. Blender was also used to UV map the pyramid and chest.

The graphics are done using a script to detect whether an object has a texture and/or normal map, and to apply the appropriate shader.
Blinn-Phong is used together with a fog effect for most objects; for a couple others including the chest, lava, and shop, a surface shader is used to create a glowing edge effect.

Camera motion is handled using various preset locations for the various rooms, with a linear interpolation between the positions and rotations.