const fs = require('fs');
const path = require('path');

function walkDir(dir, callback) {
    fs.readdirSync(dir).forEach(f => {
        let dirPath = path.join(dir, f);
        let isDirectory = fs.statSync(dirPath).isDirectory();
        isDirectory ? walkDir(dirPath, callback) : callback(path.join(dir, f));
    });
}

walkDir('./src/app', function(filePath) {
    if (filePath.endsWith('.html')) {
        let content = fs.readFileSync(filePath, 'utf8');
        // Match [class.something-[...]]="condition"
        let modified = content.replace(/\[class\.([^\]]+\[[^\]]+\][^\]]*)\]="([^"]+)"/g, `[ngClass]="{'$1': $2}"`);
        
        // Also match [class.text-[var(--mq-navy)]]="condition" 
        // Wait, the regex above handles one level of brackets. 
        // A safer regex for any class containing '[':
        modified = modified.replace(/\[class\.([a-zA-Z0-9\-\_]+\[[a-zA-Z0-9\-\_\(\)]+\][a-zA-Z0-9\-\_]*)\]="([^"]+)"/g, `[ngClass]="{'$1': $2}"`);
        
        // Even simpler: just replace ALL [class.X]="Y" where X contains '['
        modified = modified.replace(/\[class\.([^=\s]+)\]="([^"]+)"/g, (match, className, condition) => {
            if (className.includes('[')) {
                return `[ngClass]="{'${className}': ${condition}}"`;
            }
            return match;
        });

        if (content !== modified) {
            fs.writeFileSync(filePath, modified, 'utf8');
            console.log('Fixed', filePath);
        }
    }
});
