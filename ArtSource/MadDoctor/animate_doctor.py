import bpy, math, os, json
from mathutils import Vector
scene=bpy.context.scene
rig=bpy.data.objects['MadDoctor'];mesh=bpy.data.objects['MadDoctor_Mesh']
OUT=r'C:/Users/gorsk/Documents/Codex/2026-09-12/skonfigurujmy-razem-zaplanowane-zadanie-najpierw-wyja/outputs'
ART=r'C:/Projects/BaseDefenseRoguelite/BaseDefenseRoguelite/Assets/_Game/Art/MadDoctor'
def key(frame,rotations={},locs={}):
    for pb in rig.pose.bones:
        pb.rotation_euler=[math.radians(v) for v in rotations.get(pb.name,(0,0,0))]
        pb.location=locs.get(pb.name,(0,0,0))
        pb.keyframe_insert('rotation_euler',frame=frame,group=pb.name)
        pb.keyframe_insert('location',frame=frame,group=pb.name)
def action(name,frames):
    rig.animation_data_create();a=bpy.data.actions.new(name);rig.animation_data.action=a
    for f,r,l in frames:key(f,r,l)
    a.use_fake_user=True
    return a
idle=action('Idle',[(1,{'Arm.L':(-8,0,-8),'Arm.R':(-8,0,8)},{}),(31,{'Chest':(2,0,0),'Head':(-3,0,3),'Arm.L':(-10,0,-10),'Arm.R':(-10,0,10)},{'Chest':(0,.025,0)}),(61,{'Arm.L':(-8,0,-8),'Arm.R':(-8,0,8)}, {})])
action('Run',[(1,{'Thigh.L':(-32,0,0),'Thigh.R':(32,0,0),'Shin.R':(-35,0,0),'Arm.L':(25,0,-8),'Arm.R':(-30,0,8),'Chest':(8,0,4)},{}),(5,{'Shin.L':(-20,0,0),'Shin.R':(-20,0,0),'Chest':(8,0,0)},{'Pelvis':(0,.06,0)}),(9,{'Thigh.L':(32,0,0),'Thigh.R':(-32,0,0),'Shin.L':(-35,0,0),'Arm.L':(-30,0,-8),'Arm.R':(25,0,8),'Chest':(8,0,-4)},{}),(13,{'Shin.L':(-20,0,0),'Shin.R':(-20,0,0),'Chest':(8,0,0)},{'Pelvis':(0,.06,0)}),(17,{'Thigh.L':(-32,0,0),'Thigh.R':(32,0,0),'Shin.R':(-35,0,0),'Arm.L':(25,0,-8),'Arm.R':(-30,0,8),'Chest':(8,0,4)}, {})])
# Actions share anticipation at .35 and contact at .5 normalized time.
action('Throw',[(1,{},{}),(11,{'Chest':(-8,0,-18),'Arm.R':(100,0,20),'Forearm.R':(-70,0,0),'Arm.L':(-25,0,-20)},{}),(16,{'Chest':(14,0,16),'Arm.R':(-95,0,5),'Forearm.R':(-15,0,0),'Arm.L':(15,0,-10)},{}),(31,{}, {})])
action('Place',[(1,{},{}),(11,{'Chest':(22,0,0),'Head':(15,0,0),'Arm.R':(-45,0,0),'Forearm.R':(-30,0,0),'Thigh.L':(-15,0,0),'Shin.L':(-20,0,0)},{'Pelvis':(0,-.10,0)}),(16,{'Chest':(40,0,0),'Head':(12,0,0),'Arm.R':(-25,0,0)},{'Pelvis':(0,-.15,0)}),(31,{}, {})])
action('Detonate',[(1,{},{}),(11,{'Arm.L':(-70,0,-20),'Forearm.L':(-50,0,0),'Arm.R':(-65,0,20),'Forearm.R':(-85,0,0),'Head':(12,0,0)},{}),(16,{'Arm.L':(-70,0,-20),'Forearm.L':(-40,0,0),'Arm.R':(-45,0,20),'Forearm.R':(-65,0,0),'Chest':(8,0,0)},{}),(31,{}, {})])
action('Kick',[(1,{},{}),(11,{'Thigh.R':(30,0,0),'Shin.R':(-85,0,0),'Arm.L':(-25,0,-25),'Chest':(10,0,0)},{}),(16,{'Thigh.R':(-95,0,0),'Shin.R':(-5,0,0),'Chest':(-15,0,0),'Arm.L':(-35,0,-30),'Arm.R':(35,0,20)},{}),(31,{}, {})])
action('Cast',[(1,{},{}),(11,{'Arm.L':(-140,0,-30),'Arm.R':(-140,0,30),'Head':(-18,0,0)},{}),(16,{'Arm.L':(-100,0,-55),'Arm.R':(-100,0,55),'Chest':(12,0,0)},{}),(31,{}, {})])
action('Death',[(1,{},{}),(8,{'Chest':(-20,0,12),'Head':(-20,0,0),'Arm.L':(-25,0,-35),'Arm.R':(-25,0,35)},{}),(24,{'Root':(-85,0,8),'Thigh.L':(-20,0,0),'Shin.L':(-45,0,0),'Arm.L':(-15,0,-50),'Arm.R':(-15,0,50)},{'Root':(0,.18,0)}),(31,{'Root':(-90,0,8),'Thigh.L':(-20,0,0),'Shin.L':(-45,0,0),'Arm.L':(-15,0,-50),'Arm.R':(-15,0,50)},{'Root':(0,.18,0)})])
rig.animation_data.action=idle;scene.frame_set(1)
bpy.ops.object.select_all(action='DESELECT');rig.select_set(True);mesh.select_set(True);bpy.context.view_layer.objects.active=rig
bpy.ops.export_scene.fbx(filepath=ART+'/MadDoctor.fbx',use_selection=True,object_types={'ARMATURE','MESH'},add_leaf_bones=False,axis_forward='-Z',axis_up='Y',bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0,mesh_smooth_type='OFF',use_mesh_modifiers=True)
# Studio objects are outside the game export.
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.025));floor=bpy.context.object;floor.name='Studio_Floor'
m=bpy.data.materials.new('Studio_Navy');m.diffuse_color=(.027,.044,.065,1);floor.data.materials.append(m)
world=bpy.data.worlds.new('Doctor_Studio_World');scene.world=world;world.use_nodes=True
next(n for n in world.node_tree.nodes if n.type=='BACKGROUND').inputs[0].default_value=(.12,.17,.22,1)
next(n for n in world.node_tree.nodes if n.type=='BACKGROUND').inputs[1].default_value=.4
def area(name,pos,power,size,color):
    d=bpy.data.lights.new(name,'AREA');d.energy=power;d.shape='DISK';d.size=size;d.color=color
    o=bpy.data.objects.new(name,d);scene.collection.objects.link(o);o.location=pos;o.rotation_euler=(Vector((0,0,1))-o.location).to_track_quat('-Z','Y').to_euler()
area('Key',(-3,-4,6),650,4,(.80,.91,1))
area('Rim',(2,2,4),850,3,(.2,1,.70))
area('Warm_fill',(4,-2,2),350,3,(1,.56,.32))
camd=bpy.data.cameras.new('Doctor_Portrait');cam=bpy.data.objects.new('Doctor_Portrait',camd);scene.collection.objects.link(cam)
cam.location=(3,-6,3.2);cam.rotation_euler=(Vector((0,0,1.13))-cam.location).to_track_quat('-Z','Y').to_euler();camd.type='ORTHO';camd.ortho_scale=3.25;scene.camera=cam
scene.render.engine='CYCLES';scene.cycles.samples=32
scene.render.resolution_x=900;scene.render.resolution_y=1000;scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG';scene.render.filepath=OUT+'/mad-doctor-blender.png'
tex=bpy.data.images.get('MadDoctor_Palette');tex.pack()
bpy.ops.wm.save_as_mainfile(filepath=OUT+'/MadDoctor.blend')
bpy.ops.render.render(write_still=True)
print('Exported 8 animations, FBX, palette and editable Blender source.')
