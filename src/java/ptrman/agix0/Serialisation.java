package ptrman.agix0;

import com.google.gson.Gson;
import ptrman.agix0.Neuroids.Datastructures.NeuroidNetworkDescriptor;

import java.io.BufferedWriter;
import java.io.FileWriter;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Paths;

// TODO< move to place which is accesiable both by the eolution control and the Spinglass Explorer
/**
 *
 */
public class Serialisation {
    public static NeuroidNetworkDescriptor loadNetworkFromFilepath(final String filepath) {
        Gson gson = new Gson();

        String fileContent = null;
        try {
            fileContent = new String(Files.readAllBytes(Paths.get((filepath))));
        } catch (IOException e) {
            throw new RuntimeException("Can't read file \"" + filepath + "\"");
        }

        return gson.fromJson(fileContent, NeuroidNetworkDescriptor.class);
    }

    public static void saveNetworkToFilepath(final NeuroidNetworkDescriptor network, final String filepath) {
        Gson gson = new Gson();
        String jsonText = gson.toJson(network);

        BufferedWriter writer = null;
        try {
            writer = new BufferedWriter( new FileWriter( filepath));
            writer.write(jsonText);

        }
        catch (IOException e) {
            throw new RuntimeException("Can't write file \"" + filepath + "\"");
        }
        finally {
            try {
                if(writer != null) {
                    writer.close();
                }
            }
            catch(IOException e) {
            }
        }
    }
}
