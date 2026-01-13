import bpy
import os
import traceback
from pathlib import Path

""" Export and preprocess n objects from import_path to export_path. """
import_path = "/home/artursk/Desktop/Arturs_Klavins/Arturs_Klavins/Riga_Part_13"
export_path = "/home/artursk/Desktop/Arturs_Klavins/Arturs_Klavins/Riga_Part_13/export/"

n = 5
skip_existing = False  # Set to True to skip already exported files

# Create export directory if it doesn't exist
os.makedirs(export_path, exist_ok=True)

subdirs = [d for d in os.listdir(import_path)
           if os.path.isdir(os.path.join(import_path, d))]
subdirs = sorted(subdirs)[:n]

print(f"Processing {len(subdirs)} subdirectories (first {n} of {len([d for d in os.listdir(import_path) if os.path.isdir(os.path.join(import_path, d))])}):")
for sd in subdirs:
    print(f"  - {sd}")

processed = []
failed = []
total_files = 0

for subdir in subdirs:
    subdir_path = os.path.join(import_path, subdir)
    files = [f for f in os.listdir(subdir_path) if f.endswith(".obj")]

    if not files:
        print(f"No .obj files found in {subdir}")
        continue

    total_files += len(files)
    print(f"\n{subdir}: Found {len(files)} .obj file(s)")

    for file in files:
        filepath = os.path.join(subdir_path, file)
        obj_name = os.path.splitext(file)[0]
        full_export_path = os.path.join(export_path, f"{obj_name}.fbx")
        
        try:
            # Skip if already exported (only if skip_existing is True)
            if skip_existing and os.path.exists(full_export_path):
                print(f"SKIP (exists): {subdir}/{file} -> {obj_name}.fbx")
                processed.append(obj_name)
                continue
            
            # Import OBJ
            before = set(bpy.context.scene.objects)
            bpy.ops.wm.obj_import(filepath=filepath)
            after = set(bpy.context.scene.objects)
            new_objects = list(after - before)
            
            if not new_objects:
                print(f"ERROR: No objects imported from {subdir}/{file}")
                failed.append(f"{subdir}/{file}")
                continue
            
            # Merge if needed
            merged_obj = None
            if len(new_objects) > 1:
                bpy.ops.object.select_all(action='DESELECT')
                for obj in new_objects:
                    obj.select_set(True)
                bpy.context.view_layer.objects.active = new_objects[0]
                bpy.ops.object.join()
                merged_obj = bpy.context.active_object
            else:
                merged_obj = new_objects[0]
            
            if merged_obj:
                obj_name_temp = merged_obj.name
                
                # Run preprocessing
                try:
                    exec(bpy.data.texts["preprocess.py"].as_string(), globals())
                    merged_obj = bpy.context.active_object
                    
                    # Validate object after preprocessing
                    if not merged_obj or merged_obj.type != 'MESH' or len(merged_obj.data.vertices) == 0:
                        print(f"ERROR: Object became invalid after preprocessing for {subdir}/{file}")
                        failed.append(f"{subdir}/{file}")
                        try:
                            bpy.ops.object.select_all(action='SELECT')
                            bpy.ops.object.delete()
                        except:
                            pass
                        continue
                    
                    if not merged_obj or merged_obj.name != obj_name_temp:
                        merged_obj = bpy.data.objects.get(obj_name_temp)
                        
                except Exception as e:
                    print(f"ERROR: Preprocessing failed for {subdir}/{file}: {e}")
                    traceback.print_exc()
                    failed.append(f"{subdir}/{file}")
                    # Clean up broken objects
                    try:
                        bpy.ops.object.select_all(action='SELECT')
                        bpy.ops.object.delete()
                    except:
                        
                        pass
                    continue
                
                if merged_obj:
                    # Export FBX
                    try:
                        bpy.ops.object.select_all(action='DESELECT')
                        merged_obj.select_set(True)
                        bpy.context.view_layer.objects.active = merged_obj
                        # Apply all transforms before export
                        bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)

                        bpy.ops.export_scene.fbx(
                            filepath=full_export_path,
                            use_selection=True,
                            path_mode='COPY',
                            embed_textures=True,
                            axis_forward='-Z',
                            axis_up='Y',
                            global_scale=1.0,
                            apply_scale_options='FBX_SCALE_NONE',
                            apply_unit_scale=True,
                            bake_space_transform=True  # Bake axis conversion into mesh
                        )
                        bpy.data.objects.remove(merged_obj, do_unlink=True)
                        print(f"OK: {subdir}/{file} -> {obj_name}.fbx")
                        processed.append(obj_name)
                    except Exception as e:
                        print(f"ERROR: FBX export failed for {subdir}/{file}: {e}")
                        traceback.print_exc()
                        failed.append(f"{subdir}/{file}")
                        try:
                            bpy.data.objects.remove(merged_obj, do_unlink=True)
                        except:
                            pass
                else:
                    print(f"ERROR: Could not find merged object for {subdir}/{file}")
                    failed.append(f"{subdir}/{file}")
            else:
                print(f"ERROR: No merged object created for {subdir}/{file}")
                failed.append(f"{subdir}/{file}")
                
        except Exception as e:
            print(f"ERROR: Failed to process {subdir}/{file}: {e}")
            traceback.print_exc()
            failed.append(f"{subdir}/{file}")
            # Try to clean up
            try:
                bpy.ops.object.select_all(action='SELECT')
                bpy.ops.object.delete()
            except:
                pass

print(f"\n=== SUMMARY ===")
print(f"Total files found: {total_files}")
print(f"Successfully processed: {len(processed)}")
print(f"Failed: {len(failed)}")
print(f"Success rate: {len(processed)}/{total_files} ({100*len(processed)/total_files if total_files > 0 else 0:.1f}%)")
if failed:
    print(f"\nFailed files:")
    for f in failed:
        print(f"  - {f}")
