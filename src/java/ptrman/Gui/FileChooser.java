package ptrman.Gui;

import javax.swing.*;
import java.awt.*;
import java.io.File;

/**
 *
 */
public class FileChooser {
    public File load(Component parent) {
        JFileChooser fileChooser = new JFileChooser();
        fileChooser.setCurrentDirectory(new File(System.getProperty("user.home")));
        int result = fileChooser.showOpenDialog(parent);
        if (result == JFileChooser.APPROVE_OPTION) {
            File selectedFile = fileChooser.getSelectedFile();
            System.out.println("Selected file: " + selectedFile.getAbsolutePath());

            return selectedFile;
        }

        return null;
    }
}
