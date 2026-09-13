import bpy, math, os, json
from mathutils import Vector

OUT = r'C:/Users/gorsk/Documents/Codex/2026-09-12/skonfigurujmy-razem-zaplanowane-zadanie-najpierw-wyja/outputs'
ART = r'C:/Projects/BaseDefenseRoguelite/BaseDefenseRoguelite/Assets/_Game/Art/MadDoctor'
os.makedirs(ART, exist_ok=True)
if bpy.context.mode != 'OBJECT': bpy.ops.object.mode_set(mode='OBJECT')
scene = bpy.data.scenes.new('MadDoctor_Studio')
bpy.context.window.scene = scene
parts=[]
palette=[('Coat',(0.82,0.9,0.82,1)),('Shadow',(0.36,0.53,0.50,1)),('Skin',(0.83,0.50,0.28,1)),('Hair',(0.96,0.98,0.8,1)),('Graphite',(0.045,0.065,0.10,1)),('Purple',(0.30,0.09,0.43,1)),('Mint',(0.10,0.92,0.59,1)),('Orange',(1.0,0.30,0.055,1))]
materials=[]
for name,color in palette:
    m=bpy.data.materials.new('Doctor_'+name); m.diffuse_color=color; m.use_nodes=True
    next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED').inputs['Base Color'].default_value=color
    next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED').inputs['Roughness'].default_value=.73
    materials.append(m)
def finish(o,name,mat,bone):
    o.name=name; o.data.materials.append(materials[mat])
    g=o.vertex_groups.new(name=bone); g.add(list(range(len(o.data.vertices))),1,'REPLACE')
    parts.append(o); return o
def ell(name,p,s,mat,bone,seg=8,rings=4):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=seg,ring_count=rings,radius=1,location=p)
    o=bpy.context.object;o.scale=s
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    return finish(o,name,mat,bone)
def box(name,p,s,mat,bone):
    bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=bpy.context.object;o.scale=s
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    return finish(o,name,mat,bone)
def rod(name,a,b,r1,r2,mat,bone,n=8):
    a,b=Vector(a),Vector(b)
    bpy.ops.mesh.primitive_cone_add(vertices=n,radius1=r1,radius2=r2,depth=(b-a).length,location=(a+b)/2)
    o=bpy.context.object;o.rotation_euler=(b-a).to_track_quat('Z','Y').to_euler()
    bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
    return finish(o,name,mat,bone)
def rings(name,levels,mat,bone,n=8):
    verts=[(math.cos(i*2*math.pi/n)*rx,math.sin(i*2*math.pi/n)*ry,z) for z,rx,ry in levels for i in range(n)]
    faces=[tuple(reversed(range(n)))]
    for k in range(len(levels)-1):
        for i in range(n): faces.append((k*n+i,k*n+(i+1)%n,(k+1)*n+(i+1)%n,(k+1)*n+i))
    faces.append(tuple((len(levels)-1)*n+i for i in range(n)))
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update()
    o=bpy.data.objects.new(name,mesh);scene.collection.objects.link(o);return finish(o,name,mat,bone)

# A broad coat, tiny legs and oversized head keep the silhouette readable overhead.
rings('Tailored_coat',[(.68,.34,.23),(.94,.29,.20),(1.25,.31,.20),(1.46,.40,.23),(1.52,.25,.17)],0,'Chest')
box('Dark_shirt',(0,-.195,1.30),(.18,.045,.34),4,'Chest')
for side in [-1,1]:
    lapel=box('Lapel',(side*.12,-.225,1.37),(.12,.055,.28),0,'Chest');lapel.rotation_euler.y=side*-.38
    box('Pocket',(side*.24,-.195,1.03),(.13,.065,.14),1,'Chest')
box('Hazard_tie',(0,-.237,1.24),(.07,.045,.19),7,'Chest')
for z in [.85,.98,1.10]: ell('Coat_button',(0,-.225,z),(.026,.018,.026),4,'Chest',6,3)
rod('Neck',(0,0,1.47),(0,0,1.60),.11,.12,2,'Head')
ell('Head',(0,-.015,1.79),(.30,.235,.31),2,'Head',10,5)
ell('Hair_cap',(0,.045,1.98),(.31,.235,.17),3,'Head',10,4)
for i in range(7):
    a=(i-3)*.43
    start=(math.sin(a)*.22,.04+abs(i-3)*.019,2.02-abs(i-3)*.02)
    end=(math.sin(a)*.44,.09,2.34-abs(i-3)*.065)
    rod('Wild_hair',start,end,.105,.006,3,'Head',5)
for side in [-1,1]:
    ell('Ear',(side*.29,.005,1.80),(.067,.065,.11),2,'Head',6,3)
    rod('Goggle_housing',(side*.145,-.17,1.86),(side*.145,-.29,1.86),.13,.13,4,'Head',10)
    rod('Goggle_lens',(side*.145,-.294,1.86),(side*.145,-.313,1.86),.097,.097,6,'Head',10)
    box('Lens_glint',(side*.145-.023,-.327,1.90),(.036,.015,.048),3,'Head')
    box('Goggle_strap',(side*.28,.015,1.86),(.025,.32,.065),4,'Head')
rod('Nose',(0,-.21,1.78),(0,-.37,1.72),.06,.032,2,'Head',5)
box('Crooked_grin',(0,-.223,1.65),(.16,.04,.065),4,'Head')
box('Teeth',(0,-.247,1.668),(.135,.025,.022),3,'Head')
for side in [-1,1]:
    suf='L' if side<0 else 'R'
    rod('Sleeve_'+suf,(side*.33,0,1.43),(side*.48,0,1.12),.15,.11,0,'Arm.'+suf)
    rod('Cuff_'+suf,(side*.48,0,1.12),(side*.50,-.025,1.00),.115,.10,5,'Forearm.'+suf)
    ell('Glove_'+suf,(side*.51,-.045,.94),(.115,.10,.125),5,'Forearm.'+suf)
    ell('Thumb_'+suf,(side*.44,-.10,.96),(.052,.06,.075),5,'Forearm.'+suf,6,3)
    rod('Trouser_'+suf,(side*.16,0,.77),(side*.17,0,.40),.13,.10,4,'Thigh.'+suf)
    rod('Boot_shaft_'+suf,(side*.17,0,.40),(side*.17,0,.16),.115,.12,5,'Shin.'+suf)
    ell('Boot_'+suf,(side*.17,-.095,.12),(.15,.235,.12),4,'Shin.'+suf)
    box('Boot_buckle_'+suf,(side*.17,-.11,.32),(.10,.05,.06),7,'Shin.'+suf)
# Two cartoon charges at the hip and a small reactor on the back.
for side in [-1,1]:
    ell('Belt_bomb',(side*.29,-.21,.80),(.115,.11,.12),4,'Chest')
    rod('Bomb_fuse',(side*.29,-.21,.90),(side*.31,-.23,1.00),.016,.01,7,'Chest',5)
box('Reactor_pack',(0,.26,1.22),(.37,.21,.40),4,'Chest')
rod('Reactor_core',(0,.385,1.10),(0,.385,1.36),.095,.095,6,'Chest',8)
box('Reactor_cap',(0,.37,1.39),(.24,.16,.055),7,'Chest')

bpy.ops.object.select_all(action='DESELECT')
for o in parts:o.select_set(True)
bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join()
mesh=bpy.context.object;mesh.name='MadDoctor_Mesh'
# Palette UVs mean a single material and a single draw call in Unity.
uv=mesh.data.uv_layers.new(name='PaletteUV')
for face in mesh.data.polygons:
    mat=mesh.data.materials[face.material_index]
    idx=materials.index(mat)
    for li in face.loop_indices:uv.data[li].uv=((idx+.5)/8,.5)
for old_uv in list(mesh.data.uv_layers):
    if old_uv.name!='PaletteUV':mesh.data.uv_layers.remove(old_uv)
mesh.data.uv_layers.active_index=0;mesh.data.uv_layers[0].active_render=True
tex=bpy.data.images.new('MadDoctor_Palette',width=64,height=8,alpha=True)
pixels=[]
for y in range(8):
    for x in range(64):
        c=palette[x//8][1]
        # image pixels are scene linear, saved image uses sRGB conversion.
        pixels.extend(c)
tex.pixels=pixels;tex.filepath_raw=ART+'/MadDoctor_Palette.png';tex.file_format='PNG';tex.save()
mat=bpy.data.materials.new('MadDoctor_Palette');mat.use_nodes=True
node=mat.node_tree.nodes.new('ShaderNodeTexImage');node.image=tex;node.interpolation='Closest'
mat.node_tree.links.new(node.outputs['Color'],next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED').inputs['Base Color'])
next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED').inputs['Roughness'].default_value=.75
mesh.data.materials.clear();mesh.data.materials.append(mat)
for p in mesh.data.polygons:p.material_index=0
mesh.data.calc_loop_triangles();print('TRIANGLES',len(mesh.data.loop_triangles))
assert len(mesh.data.loop_triangles)<=2000

bpy.ops.object.select_all(action='DESELECT')
armdata=bpy.data.armatures.new('MadDoctor_Rig');rig=bpy.data.objects.new('MadDoctor',armdata);scene.collection.objects.link(rig)
bpy.context.view_layer.objects.active=rig;rig.select_set(True);bpy.ops.object.mode_set(mode='EDIT')
bones={'Root':((0,0,0),None),'Pelvis':((0,0,.76),'Root'),'Chest':((0,0,1.13),'Pelvis'),'Head':((0,0,1.55),'Chest')}
for side in [-1,1]:
    s='L' if side<0 else 'R'
    bones['Arm.'+s]=((side*.34,0,1.43),'Chest')
    bones['Forearm.'+s]=((side*.48,0,1.12),'Arm.'+s)
    bones['Thigh.'+s]=((side*.16,0,.76),'Pelvis')
    bones['Shin.'+s]=((side*.17,0,.40),'Thigh.'+s)
for name,(pos,parent) in bones.items():
    b=armdata.edit_bones.new(name);b.head=pos;b.tail=Vector(pos)+Vector((0,0,.12))
    if parent:b.parent=armdata.edit_bones[parent]
bpy.ops.object.mode_set(mode='OBJECT')
mesh.parent=rig;mod=mesh.modifiers.new('Doctor_skin','ARMATURE');mod.object=rig
rig.show_in_front=True
for pb in rig.pose.bones:pb.rotation_mode='XYZ'
scene.render.fps=30
print('Created doctor:',len(mesh.data.vertices),'vertices;',len(bones),'bones')

