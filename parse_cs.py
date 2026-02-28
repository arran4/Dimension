import os
import re

def parse_cs_file(filepath):
    classes = []
    methods = []
    properties = []

    with open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
        content = f.read()

        # Regex to find class names
        class_pattern = re.compile(r'class\s+([A-Za-z0-9_]+)')
        classes.extend(class_pattern.findall(content))

        # Regex to find public methods (simplified)
        method_pattern = re.compile(r'public\s+(?:static\s+)?(?:virtual\s+|override\s+)?[A-Za-z0-9_<>\[\]]+\s+([A-Za-z0-9_]+)\s*\(')
        methods.extend(method_pattern.findall(content))

        # Regex to find public properties (simplified)
        prop_pattern = re.compile(r'public\s+(?:static\s+)?[A-Za-z0-9_<>\[\]]+\s+([A-Za-z0-9_]+)\s*\{\s*get')
        properties.extend(prop_pattern.findall(content))

    return classes, methods, properties

def generate_todo(output_file='TODO.md'):
    with open(output_file, 'w', encoding='utf-8') as f:
        f.write('# Dimension Flutter Port TODO List\n\n')
        f.write('This document outlines the tasks required to port the C# application to a Dart/Flutter application.\n\n')

        for root, dirs, files in os.walk('.'):
            # Exclude dot directories like .git
            dirs[:] = [d for d in dirs if not d.startswith('.')]

            for file in files:
                if file.endswith('.cs'):
                    filepath = os.path.join(root, file)
                    classes, methods, properties = parse_cs_file(filepath)

                    if classes or methods or properties:
                        f.write(f'## File: `{filepath}`\n\n')
                        f.write(f'- [ ] Port `{file}` to Dart\n')

                        if classes:
                            f.write('  - **Classes**:\n')
                            for c in classes:
                                f.write(f'    - [ ] `class {c}`\n')

                        if methods:
                            f.write('  - **Public Methods**:\n')
                            for m in methods:
                                f.write(f'    - [ ] `{m}()`\n')

                        if properties:
                            f.write('  - **Public Properties**:\n')
                            for p in properties:
                                f.write(f'    - [ ] `{p}`\n')

                        f.write('\n')

if __name__ == '__main__':
    generate_todo()
