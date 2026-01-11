import bpy
import math

def rotate(obj):
    bpy.ops.object.empty_add(type='PLAIN_AXES', location=(0, 0, 0))
    empty = bpy.context.active_object
    
    obj.parent = empty
    empty.rotation_euler.x = math.radians(-90)
    
    bpy.ops.object.select_all(action='DESELECT')
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    
    bpy.ops.object.parent_clear(type='CLEAR_KEEP_TRANSFORM')
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
    bpy.data.objects.remove(empty, do_unlink=True)

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
        return
    
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
    
def offset_to_origin(obj, offset_x, offset_z):
    """Offset object to origin. In Blender, X is X and Y is Z (different axes than Unity)"""
    bpy.ops.object.select_all(action='DESELECT')
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT')
    
    # In Blender: X=X, Y=Up, Z=-Forward
    # Unity X offset -> Blender X offset
    # Unity Z offset -> Blender -Y offset (because Unity Z is Blender's -Y looking from above)
    bpy.ops.transform.translate(value=(-offset_x, 0, offset_z))
    
    bpy.ops.object.mode_set(mode='OBJECT')
    print(f"Offset {obj.name} by X: -{offset_x}, Z: +{offset_z}")

# Execute preprocessing on the merged_obj
print(f"Starting preprocessing on {merged_obj.name}")
offset_to_origin(merged_obj, 50689.09, 31198.79) # Milda zero point
#rotate(merged_obj)
retopology(merged_obj)
clean_up(merged_obj)
print(f"Preprocessing complete. Active object: {bpy.context.active_object.name}")
