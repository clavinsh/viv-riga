import bpy
import os

""" Export and preprocess n objects from import_path to export_path. """
import_path = ""
export_path = ""
n = 3

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
                obj_name_temp = merged_obj.name
                exec(bpy.data.texts["preprocess.py"].as_string(), globals())
                merged_obj = bpy.context.active_object

                if not merged_obj or merged_obj.name != obj_name_temp:
                    merged_obj = bpy.data.objects.get(obj_name_temp)

                if merged_obj:
                    obj_name = os.path.splitext(file)[0]
                    export_path = os.path.join(export_path, f"{obj_name}.fbx")

                    bpy.ops.object.select_all(action='DESELECT')
                    merged_obj.select_set(True)
                    bpy.context.view_layer.objects.active = merged_obj

                    bpy.ops.export_scene.fbx(
                        filepath = export_path, 
                        use_selection = True,
                        path_mode = 'COPY',
                        embed_textures = True,
                        axis_forward = '-Z',
                        axis_up = 'Y',
                        global_scale = 1.0,
                    )

                    bpy.data.objects.remove(merged_obj, do_unlink=True)
                    
