import os
import subprocess

# === CONFIGURA ESTOS DATOS ===
CORREO_MALO = "Rosamariatorresapaza871@gmail.com"
NOMBRE_CORRECTO = "MathiuZuri"
CORREO_CORRECTO = "yukyuel@gmail.com"
# =============================

script = f"""
if [ "$GIT_COMMITTER_EMAIL" = "{CORREO_MALO}" ]
then
    export GIT_COMMITTER_NAME="{NOMBRE_CORRECTO}"
    export GIT_COMMITTER_EMAIL="{CORREO_CORRECTO}"
fi
if [ "$GIT_AUTHOR_EMAIL" = "{CORREO_MALO}" ]
then
    export GIT_AUTHOR_NAME="{NOMBRE_CORRECTO}"
    export GIT_AUTHOR_EMAIL="{CORREO_CORRECTO}"
fi
"""

# Ejecutar el filtro de Git
print("Iniciando la reescritura del historial... Esto puede tardar un poco.")
subprocess.run(["git", "filter-branch", "--force", "--env-filter", script, "--tag-name-filter", "cat", "--", "--all"])
print("¡Historial limpio localmente!")