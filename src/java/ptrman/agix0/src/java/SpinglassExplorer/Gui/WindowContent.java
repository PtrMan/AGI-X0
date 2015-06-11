package ptrman.agix0.src.java.SpinglassExplorer.Gui;

import ptrman.agix0.src.java.Evolvator.Evironment.Playground;

import javax.swing.*;
import java.awt.*;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;

/**
 *
 */
public class WindowContent extends JPanel {
    private class LoadNetworkButtonPressed implements ActionListener {
        @Override
        public void actionPerformed(ActionEvent e) {
            // TODO
            int todo = 0;
        }
    }

    public WindowContent(Playground playground) {
        super();

        this.environment2dCanvas = new Environment2dCanvas(playground);

        buildGui();
        setSize(1024, 1000);
        setVisible(true);
    }

    private void buildGui() {
        setLayout(new GridLayout(3, 1));

        // TODO< replace with load setup javascript file which just loads and configures the environment and the network? >
        JButton loadButton = new JButton("load network");
        loadButton.addActionListener(new LoadNetworkButtonPressed());
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
