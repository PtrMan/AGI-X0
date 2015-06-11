package ptrman.agix0.src.java.SpinglassExplorer.Gui;

import ptrman.Gui.FileChooser;
import ptrman.agix0.src.java.Evolvator.Evironment.Playground;

import javax.swing.*;
import java.awt.*;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.io.File;

/**
 *
 */
public class WindowContent extends JPanel {
    private final ILoadNetworkHandler loadNetworkHandler;

    private class LoadNetworkButtonPressed implements ActionListener {
        private final WindowContent windowContent;
        private final ILoadNetworkHandler loadNetworkHandler;

        public LoadNetworkButtonPressed(WindowContent windowContent, ILoadNetworkHandler loadNetworkHandler) {
            this.windowContent = windowContent;
            this.loadNetworkHandler = loadNetworkHandler;
        }

        @Override
        public void actionPerformed(ActionEvent e) {
            FileChooser fileChooser = new FileChooser();
            File fileToLoad = fileChooser.load(windowContent);

            loadNetworkHandler.load(fileToLoad.getAbsolutePath());
        }
    }

    public interface ILoadNetworkHandler {
        void load(String filepath);
    }

    public WindowContent(Playground playground, ILoadNetworkHandler loadNetworkHandler) {
        super();

        this.environment2dCanvas = new Environment2dCanvas(playground);
        this.loadNetworkHandler = loadNetworkHandler;

        buildGui();
        setSize(1024, 1000);
        setVisible(true);
    }

    private void buildGui() {
        setLayout(new GridLayout(3, 1));

        // TODO< replace with load setup javascript file which just loads and configures the environment and the network? >
        JButton loadButton = new JButton("load network");
        loadButton.addActionListener(new LoadNetworkButtonPressed(this, loadNetworkHandler));
        add(loadButton);

        environment2dCanvas.setSize(200, 200);
        add(environment2dCanvas);

        networkCanvas = new SingleNetworkCanvas();
        networkCanvas.setSize(200, 200);

        add(networkCanvas);


    }

    public Environment2dCanvas environment2dCanvas;
    public SingleNetworkCanvas networkCanvas;
}
