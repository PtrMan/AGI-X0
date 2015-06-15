package ptrman.agix0.src.java.SpinglassExplorer.Gui;

import ptrman.Gui.FileChooser;
import ptrman.agix0.src.java.Common.Evironment.Environment;

import javax.swing.*;
import java.awt.*;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.io.File;

/**
 *
 */
public class WindowContent extends JPanel {
    private final ILoadSetupScriptHandler loadNetworkHandler;

    private class LoadSetupScriptButtonPressed implements ActionListener {
        private final WindowContent windowContent;
        private final ILoadSetupScriptHandler loadSetupScriptHandler;

        public LoadSetupScriptButtonPressed(WindowContent windowContent, ILoadSetupScriptHandler loadNetworkHandler) {
            this.windowContent = windowContent;
            this.loadSetupScriptHandler = loadNetworkHandler;
        }

        @Override
        public void actionPerformed(ActionEvent e) {
            FileChooser fileChooser = new FileChooser();
            File fileToLoad = fileChooser.load(windowContent);

            loadSetupScriptHandler.load(fileToLoad.getAbsolutePath());
        }
    }

    public interface ILoadSetupScriptHandler {
        void load(String filepath);
    }

    public WindowContent(Environment environment, ILoadSetupScriptHandler loadNetworkHandler) {
        super();

        this.environment2dCanvas = new Environment2dCanvas(environment);
        this.loadNetworkHandler = loadNetworkHandler;

        buildGui();
        setSize(1024, 1000);
        setVisible(true);
    }

    private void buildGui() {
        setLayout(new GridLayout(3, 1));

        // TODO< replace with load setup javascript file which just loads and configures the environment and the network? >
        JButton loadButton = new JButton("load setup script");
        loadButton.addActionListener(new LoadSetupScriptButtonPressed(this, loadNetworkHandler));
        add(loadButton);

        environment2dCanvas.setSize(200, 200);
        add(environment2dCanvas);

        networkCanvas = new SingleNetworkCanvas();
        networkCanvas.setSize(200, 200);

        add(networkCanvas);


    }

    public void actualizeAllDisplays() {
        environment2dCanvas.repaint();
        networkCanvas.repaint();
    }

    public Environment2dCanvas environment2dCanvas;
    public SingleNetworkCanvas networkCanvas;
}
