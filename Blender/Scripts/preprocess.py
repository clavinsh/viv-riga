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
        # ratio = 1
    elif vertex_count > 10000:
        merge_threshold = 0.25
        ratio = 0.8
        # ratio = 1
    elif vertex_count > 5000:
        merge_threshold = 0.125
        ratio = 0.9
        # ratio = 1
    else:
        merge_threshold = 0.01
        ratio = None
    
    bpy.ops.mesh.remove_doubles(threshold=merge_threshold)
    bpy.ops.object.mode_set(mode='OBJECT')
    print(f"Merged vertices with threshold {merge_threshold}")
    
    if ratio:
        decimate = obj.modifiers.new(name="Decimate", type='DECIMATE')
        decimate.decimate_type = 'COLLAPSE'
        decimate.ratio = ratio
        bpy.ops.object.modifier_apply(modifier="Decimate")
        print(f"Decimated to {ratio * 100}% ratio")
    else:
        print("Skipped decimation (vertex count too low)")


def clean_up(obj):
    """Separate loose parts and delete all except the largest"""
    bpy.ops.object.select_all(action='DESELECT')
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT')
    
    bpy.ops.mesh.separate(type='LOOSE')
    bpy.ops.object.mode_set(mode='OBJECT')
    parts = list(bpy.context.selected_objects)
    
    parts.sort(key=lambda o: len(o.data.vertices))
    largest = parts.pop()
    
    for o in parts:
        print(f"Deleting: {o.name}")
        bpy.data.objects.remove(o)
    

rotate(merged_obj)
retopology(merged_obj)
clean_up(merged_obj) 

