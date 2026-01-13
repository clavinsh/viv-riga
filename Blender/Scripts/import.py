import bpy
import os

""" Import and preprocess n objects from import_path. """
import_path = "/home/artursk/Desktop/Arturs_Klavins/Arturs_Klavins/Riga_Part_13"
n = 10

subdirs = [d for d in os.listdir(import_path) 
           if os.path.isdir(os.path.join(import_path, d))]
subdirs = sorted(subdirs)[:n]

for subdir in subdirs:
    subdir_path = os.path.join(import_path, subdir)

    for file in os.listdir(subdir_path):
        if file.endswith(".obj"):
            filepath = os.path.join(subdir_path, file)
            before = set(bpy.context.scene.objects)
            bpy.ops.wm.obj_import(filepath=filepath)
            after = set(bpy.context.scene.objects)
            new_objects = list(after - before)
            merged_obj = None

            if len(new_objects) > 1:
                bpy.ops.object.select_all(action='DESELECT')

                for obj in new_objects:
                    obj.select_set(True)

                bpy.context.view_layer.objects.active = new_objects[0]
                bpy.ops.object.join()
                merged_obj = bpy.context.active_object 
            elif len(new_objects) == 1:
                merged_obj = new_objects[0]

            if merged_obj:
                exec(bpy.data.texts["preprocess.py"].as_string(), globals())
