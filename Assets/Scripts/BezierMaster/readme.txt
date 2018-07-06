Hello! This is Bezier Master! With it, you can easily create 3D curves and place objects along it!

To do that:
1. Create Bezier Master object, or just add Bezier Master component on your object;

2. Manipulate controll points, add or remove points to create curve that you need. 
You can also change rotation and scale in Curve Editor tab. That will affect instatiated objects;

3. In Objects Instatiating tab you can choose Objects or Mesh and parameters to it 
(like Prefabs, that you want plase(it can be more than 1), mesh resolution etc.)

4. Also you can create just curve (choose None at top of Objects Instatiating tab) and get points along it to move or something else.
Just call GetPath(pointsCount) from object with that component. It returns array of Vector3;

I hope that it will be useful for you!
With any questions and suggestions you can contact me by email: sanyaoff93@gmail.com