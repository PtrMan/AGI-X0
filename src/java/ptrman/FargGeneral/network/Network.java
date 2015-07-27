package ptrman.FargGeneral.network;

import java.util.ArrayList;
import java.util.List;

/**
 *
 * slipnet
 */
public class Network
{
    public interface IDepthToDecay
    {
        public float translateDepthToDecayrate(int depth);
    }
    
    // used to create links with the right type
    public interface ILinkCreator
    {
        public Link createLink(Link.EnumType type, Node target);
    }
    
    public List<Node> nodes = new ArrayList<>();
    public IDepthToDecay depthToDecay;
    
    public void spreadActivation()
    {
        resetActivationDelta();
        spreadActivationInternal();
        addActivationDelta();
        limitActivation();
    }
    
    public void letAllNodesDecay()
    {
        for( Node iterationNode : nodes )
        {
            float decayRate;
            
            decayRate = depthToDecay.translateDepthToDecayrate(iterationNode.conceptualDepth);
            iterationNode.activiation *= (1.0f - decayRate);
        }
    }
    
    private void resetActivationDelta()
    {
        for( Node iterationNode : nodes )
        {
            iterationNode.resetActivationDelta();
        }
    }
    
    private void spreadActivationInternal()
    {
        for( Node iterationNode : nodes )
        {
            float sourceNodeActivation;
            
            sourceNodeActivation = iterationNode.activiation;
            
            for( Link iterationLink : iterationNode.outgoingLinks )
            {
                iterationLink.target.activationDelta += (iterationLink.strength * sourceNodeActivation);
            }
        }
    }
    
    private void addActivationDelta()
    {
        for( Node iterationNode : nodes )
        {
            iterationNode.addActivationDelta();
        }
    }
    
    private void limitActivation()
    {
        for( Node iterationNode : nodes )
        {
            iterationNode.activiation = Math.min(iterationNode.activiation, 1.0f);
        }
    }
    
    public ILinkCreator linkCreator;
}
