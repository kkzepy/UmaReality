import re, json

motion_set_data = {
    "motion_set":{

    }
}

keys = {#{"fight":"excited","kuchiate":"covering_mouth","muneate":"hand_on_chest","sad":"sad","tekumi":"hands_intertwined","tired":"tired","think":"thinking","soppo":"facing_away","guard":"guard_self","see":"seeing_away","bow":"bow","come":"come_closer","hope":"hope",
 " idea": "idea",
 " hope": "hope",
 " fight": "excited",
 " soppo": "facing-away",
 " sad": "sad",
 " muneate": "hand-on-chest",
 " think": "thinking",
 " shy": "shy",
 " tekumi": "hands-intertwined",
 " surprise": "surprised",
 " kuchiate": "covering-mouth",
 " hello": "greeting",
 " handsup": "hands-up",
 " bow": "bow",
 " sneeze": "sneeze",
 " idle": "idle",
 " sleep": "sleep",
 " guard": "guard-self",
 " sudachi": "stand-straight",
 " joy": "happy",
 " koshiate": "hands-on-hip",
 " angry": "angry",
 " shock": "shock",
 " cry": "cry",
 " gatts": "confident",
 " pose": "pose",
 " akire": "idk",
 " near": "leaning-in",
 " udekumi": "folded-arms",
 " byebye": "farewell",
 " tired": "tired",
 " laugh": "laughing",
 " good": "thumbs-up",
 " peace": "peace",
 " happy": "happy",
 " advice": "advice",
 " clap": "hands-clap",
 " mot": None,
 " pain": "pain",
 " send": "lending-hands",
 " quiet": "be-quiet",
 " homestand": None,
 " hungry": "hungry",
 " see": "seeing-away",
 " ikaku": "ready",
 " come": "come-closer",
 " point": "point",
 " stretch": "stretch",
 " hot": "hot",
 " neckup": "neckup-nod",
 " oh": "excited",
 " yell": "yell",
 " joke": "joking",
 " swing": "swinging-hands",
 " shake": "shake",
 " sorry": "sorry",
 " ng": "no-gesture",
 " muscle": "muscle",
 " salute": "salute",
 " pdk": None,
 " stop": "stop",
 " request": "please",
 " set": None,
 " kiss": None,
 " stomp": "stomp",
 " hurry": "hurry",
 " act": None,
 " diarywrite": "write_diary",
 " check": None,
 " watchbase": None,
 " watchlook": None
}

{
    "FutuA": "normal",
    "Base": None,
    "KomariC": "troubled",
    "OdorokiA": "surprised",
    "KanasiC": "sad",
    "IkariA": "angry",
    "JitomeA": "disdainful-stare",
    "KusyoAL": "smirking-left",
    "WinkL": "wink-left",
    "DereA": "lovestruck",
    "OutGameWaraiB": None,
    "OdorokiC": "shocked",
    "DoyaA": "smug",
    "IkariC": "furious",
    "KanasiA": "sorrowful",
    "KomariA": "worried",
    "WaraiC": "big-smile",
    "KusyoCL": "grinning-left",
    "WaraiA": "smile"
}

def CollectAllMotset():
    with open("motset.txt","r") as motset_file:
        chars = motset_file.read().split("\n\n")
        for char in chars:
            motset = char.split("\n")

            char_id = int(motset[1][:4])
            #if char_id != 1025: continue
            
            """
            data = {
                    "excited":[], #fight
                    "covering_mouth":[], #kuchiate
                    "hand_on_chest":[], #muneate
                    "sad": [], #sad
                    "hands_intertwined":[], #tekumi
                    "tired":[], #tired,
                    "thinking":[], #think
                    "facing_away":[], #soppo
                    "guard_self":[], #guard
                    "seeing_away":[], #see
                    "bow":[], #bow
                    "come_closer":[], #come
                    "hope":[], #hope
            }
            """
            data = {}
            
            for line in motset:
                line = line.split(",")
                if len(line) == 1: continue

                for k, v in keys.items():
                    if k in line[1]:
                        if not v in data:
                            data[v]=[]

                        path = f"3d/motion/event/body/type00/anm_eve_type00_{line[1][1:].replace('_mirror','')}_loop"
                        if "_mirror" in line[1]:
                            path+="_mirror"

                        if not path in data[v]:
                            data[v].append(path)
                        break

            motion_set_data[str(char_id)] = data

def AnimsOverrideKeys():
    with open("motset.txt","r") as motset_file:
        animov_names = []

        chars = motset_file.read().split("\n\n")
        for char in chars:
            motset = char.split("\n")

            char_id = int(motset[1][:4])

            for line in motset:
                line = line.split(",")
                if len(line) == 1: continue

                name = line[1].replace("_mirror","")
                name = re.sub(r'\d', '', name)

                if not name in animov_names:
                    animov_names.append(name)

        print(animov_names)

def FaceTypeDatas():
    with open("face_type.txt","r") as ft_file:
        faced_names = ft_file.read().split("\n")

        print(set(faced_names))

#AnimsOverrideKeys()
#CollectAllMotset()
#json.dump(motion_set_data, open("motset_final.json","w"), indent=4)
FaceTypeDatas()