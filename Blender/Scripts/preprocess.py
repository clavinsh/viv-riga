import bpy
import math

def offset_to_origin(obj, offset_x, offset_z, offset_y):
#    """Offset all vertex coordinates to bring model to origin"""
    bpy.ops.object.select_all(action='DESELECT')
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj

    # Apply ALL transforms (location, rotation, scale) to bake them into vertices
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)

#    # Move vertices directly in object data (subtract to bring to origin)
    for vertex in obj.data.vertices:
        vertex.co.x -= offset_x
        vertex.co.y -= offset_z  # Y in Blender corresponds to Z in Unity (vertical axis)
        vertex.co.z -= offset_y

    # Update mesh and force dependency graph update
    obj.data.update()
    bpy.context.view_layer.update()

    # Reset object transform to identity (location, rotation, scale)
    obj.location = (0, 0, 0)
    obj.rotation_euler = (0, 0, 0)
    obj.scale = (1, 1, 1)

    # Force another update after transform reset
    bpy.context.view_layer.update()

    # Ensure object is selected and active before setting origin
    bpy.ops.object.select_all(action='DESELECT')
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj

    # Set origin to geometry - requires VIEW_3D context override
    for window in bpy.context.window_manager.windows:
        screen = window.screen
        for area in screen.areas:
            if area.type == 'VIEW_3D':
                with bpy.context.temp_override(window=window, area=area):
                    bpy.ops.object.origin_set(type='ORIGIN_GEOMETRY', center='BOUNDS')
                break

    print(f"Offset {obj.name} vertices by X: {-offset_x}, Y: {-offset_z}, Z: {-offset_y}")

def rotate(obj):
    """Rotate object around its own center, preserving world position."""
    bpy.ops.object.select_all(action='DESELECT')
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj

    # Store original world position
    original_location = obj.location.copy()

    # Rotate around object's own origin (not world origin)
    obj.rotation_euler.x += math.radians(270)

    # Apply only rotation, keep location
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)

    # Restore location (in case it shifted)
    obj.location = original_location

def retopology(obj):
    """ Merge by distance and decimate """
    bpy.ops.object.select_all(action='DESELECT')
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj

    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT')
    vertex_count = len(obj.data.vertices)
    print(f"Object {obj.name} has {vertex_count} vertices")

    if vertex_count > 50000:
        merge_threshold = 0.5
        ratio = 0.4
    elif vertex_count > 10000:
        merge_threshold = 0.25
        ratio = 1.0
    elif vertex_count > 5000:
        merge_threshold = 0.125
        ratio = 1.0
    else:
        merge_threshold = 0.01
        ratio = 1.0

    bpy.ops.mesh.remove_doubles(threshold=merge_threshold)
    bpy.ops.object.mode_set(mode='OBJECT')
    print(f"Merged vertices with threshold {merge_threshold}")

    if ratio < 1.0:
        decimate = obj.modifiers.new(name="Decimate", type='DECIMATE')
        decimate.decimate_type = 'COLLAPSE'
        decimate.ratio = ratio
        bpy.ops.object.modifier_apply(modifier="Decimate")
        print(f"Decimated to {ratio * 100}% ratio")
    else:
        print("Skipped decimation (ratio 1.0)")

def clean_up(obj):
    """Separate loose parts and delete all except the largest"""
    bpy.ops.object.select_all(action='DESELECT')
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj

    # Store original object name to track it
    original_obj_name = obj.name

    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT')

    bpy.ops.mesh.separate(type='LOOSE')
    bpy.ops.object.mode_set(mode='OBJECT')

    # Get all objects in scene and find the parts
    all_objects = bpy.context.scene.objects

    # Find parts by vertex count (the separate operation creates new objects)
    parts = [o for o in all_objects if o.type == 'MESH' and o.select_get()]

    if not parts:
        print(f"WARNING: No parts found after separation")
        return obj

    print(f"Found {len(parts)} parts after separation")

    # Sort by vertex count and keep the largest
    parts.sort(key=lambda o: len(o.data.vertices), reverse=True)
    largest = parts[0]

    print(f"Keeping largest part: {largest.name} with {len(largest.data.vertices)} vertices")

    # Delete all other parts
    for o in parts[1:]:
        print(f"Deleting: {o.name} with {len(o.data.vertices)} vertices")
        bpy.data.objects.remove(o, do_unlink=True)

    # Make sure the largest part is selected and active
    bpy.ops.object.select_all(action='DESELECT')
    largest.select_set(True)
    bpy.context.view_layer.objects.active = largest

    return largest

# Execute preprocessing on the merged_obj
print(f"Starting preprocessing on {merged_obj.name}")
rotate(merged_obj)
retopology(merged_obj)
merged_obj = clean_up(merged_obj)
offset_to_origin(merged_obj, 505701, 310890, 19.5549)
print(f"Preprocessing complete. Active object: {bpy.context.active_object.name}")
