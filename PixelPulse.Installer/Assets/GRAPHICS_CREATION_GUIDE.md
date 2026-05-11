# Installer Graphics Creation Guide

## Required Files

### 1. banner.bmp
**Location:** `PixelPulse.Installer\Assets\banner.bmp`  
**Dimensions:** 493x58 pixels  
**Format:** 24-bit BMP (required by WiX)

**Design:**
- Horizontal gradient: Purple (#6B46C1) → Blue (#3B82F6)
- Text: "Pixel Tech Solutions" in white or light color
- Font: Modern, professional, readable
- Optional: Subtle pixel pattern or tech elements
- Style: Clean, professional installer banner

### 2. dialog.bmp
**Location:** `PixelPulse.Installer\Assets\dialog.bmp`  
**Dimensions:** 493x312 pixels  
**Format:** 24-bit BMP (required by WiX)

**Design:**
- Subtle gradient using brand colors
- Colors: Purple (#6B46C1), Blue (#3B82F6), Dark Gray (#1F2937)
- Very subtle pattern or texture
- Non-distracting background
- Professional appearance

### 3. icon.ico
**Location:** `PixelPulse.Installer\Assets\icon.ico`  
**Format:** ICO file with multiple sizes

**Note:** Can be the same as the application icon (`PixelPulse\Resources\icon.ico`)

## Creation Steps

### Step 1: Create Designs
1. Use image editing software (GIMP, Photoshop, etc.)
2. Create designs at exact dimensions:
   - Banner: 493x58 pixels
   - Dialog: 493x312 pixels
3. Use brand colors consistently
4. Ensure text is readable

### Step 2: Convert to BMP Format

**Using GIMP:**
1. File → Export As
2. Change extension to .bmp
3. In export dialog, ensure "Do not write color space information" is checked
4. Save as 24-bit BMP

**Using Photoshop:**
1. File → Save As
2. Choose BMP format
3. Select "Windows" and "24-bit" options
4. Save

**Using Online Converter:**
1. Visit: https://convertio.co/png-bmp/
2. Upload PNG file
3. Convert to BMP
4. Download 24-bit BMP file

### Step 3: Verify Format
- File size should be reasonable (not too large)
- Format must be 24-bit BMP (not 32-bit or other)
- Dimensions must match exactly
- Colors should display correctly

## Design Tips

### Banner Design
- Keep text centered or left-aligned
- Use high contrast for readability
- White or light gray text works best
- Consider adding subtle glow or shadow to text
- Maintain professional appearance

### Dialog Background
- Keep it subtle - it's a background
- Don't use bright colors that distract
- Gradient should be gentle
- Consider adding very subtle texture
- Test with white text overlay to ensure readability

## Testing

After creating graphics:
1. Place files in `PixelPulse.Installer\Assets\`
2. Build installer project
3. Run installer to verify graphics display correctly
4. Check all installer dialogs:
   - Welcome page
   - License agreement
   - Install directory
   - Progress
   - Completion

## Brand Colors Reference

- **Primary Purple:** #6B46C1 (RGB: 107, 70, 193)
- **Secondary Blue:** #3B82F6 (RGB: 59, 130, 246)
- **Accent Gold:** #F59E0B (RGB: 245, 158, 11)
- **Dark Background:** #1F2937 (RGB: 31, 41, 55)
- **Light Text:** #F9FAFB (RGB: 249, 250, 251)

## Quick Templates

### Banner Template (493x58)
```
Background: Horizontal gradient Purple → Blue
Text: "Pixel Tech Solutions" (centered, white, bold)
Optional: Subtle pixel grid pattern overlay
```

### Dialog Template (493x312)
```
Background: Subtle vertical gradient
Top: Dark Gray (#1F2937)
Middle: Purple (#6B46C1) - very subtle
Bottom: Blue (#3B82F6) - very subtle
Overall: Very low opacity, non-distracting
```

## Troubleshooting

**Graphics don't display in installer:**
- Verify file format is 24-bit BMP
- Check file paths in Product.wxs
- Ensure files are in Assets folder
- Rebuild installer project

**Colors look wrong:**
- Verify RGB color values
- Check BMP color depth (must be 24-bit)
- Ensure no color profile is embedded

**Text is unreadable:**
- Increase text contrast
- Use white or light colors on dark backgrounds
- Add text shadow or outline if needed
