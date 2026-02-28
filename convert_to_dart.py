import os
import re

def to_snake_case(name):
    # Convert PascalCase/camelCase to snake_case
    s1 = re.sub('(.)([A-Z][a-z]+)', r'\1_\2', name)
    return re.sub('([a-z0-9])([A-Z])', r'\1_\2', s1).lower()

def convert_cs_to_dart():
    for root, dirs, files in os.walk('.'):
        dirs[:] = [d for d in dirs if not d.startswith('.')]

        for file in files:
            if file.endswith('.cs'):
                filepath = os.path.join(root, file)

                # Determine standard Dart path
                # Example: ./DimensionLib/Model/Commands/Command.cs -> lib/model/commands/command.dart
                rel_path = os.path.relpath(filepath, '.')
                parts = rel_path.split(os.sep)

                # Drop the root project folder like DimensionLib, Dimension, Updater
                if len(parts) > 1:
                    target_dir_parts = [p.lower() for p in parts[1:-1]]
                else:
                    target_dir_parts = []

                target_filename = to_snake_case(os.path.splitext(file)[0]) + '.dart'

                target_dir = os.path.join('lib', *target_dir_parts)
                os.makedirs(target_dir, exist_ok=True)

                target_path = os.path.join(target_dir, target_filename)

                # Read C# content
                with open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
                    cs_content = f.read()

                # Write to Dart file as a block comment
                with open(target_path, 'w', encoding='utf-8') as f:
                    f.write('/*\n')
                    f.write(f' * Original C# Source File: {rel_path}\n')
                    f.write(' * \n')
                    f.write(cs_content)
                    f.write('\n*/\n')

                # Delete the original .cs file
                os.remove(filepath)
                print(f"Converted {rel_path} -> {target_path}")

if __name__ == '__main__':
    convert_cs_to_dart()
