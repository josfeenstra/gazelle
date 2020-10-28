
# from copy import deepcopy
        
# helper function
def isfloatable(value):
  try:
    float(value)
    return True
  except:
    return False

class Node(object): 
    
    def __getattr__(self, attr):
        """  an attribute does not exists: return false """
        print("failed to collect {}".format(attr))
        return False
    
    def __getattribute__(self, attr):
        """ this magic method takes care of converting stringified floats 
            back to normal floats, once called 
        """
        value = object.__getattribute__(self, attr)
        if type(value) == str:
            
            # if the value is a string, try to cast it to a value we can use 
            if isfloatable(value):
                print("changed to float")
                value = float(value)
            
        return value
    
    def __init__(self, dictionary={}):
        self.z_add(dictionary)
        
    def z_add(self, dictionary, paths=[]):
        """  convert and integrate dictionary at a certain path of the data tree 
        """
        # paths is a list of nodes to walk through 
        if paths:
            
            # if the current path exists, add it, if not, create it
            path = paths[0]
            node = self.z_getNode(path, False)
            if node:
                
                # use existing node 
                return node.z_add(dictionary, paths[1:])
            else:
                
                # create a new node 
                self.z_addNode(path)
                
        # add the dict 
        for key, value in dictionary.iteritems(): 
            if isinstance(value, dict):
                
                # if a nested dict is found in the dict, check if it exists
                node = self.z_getNode(key, False)
                if not node:
                    
                    # if the key is not known to the Node, add it and recurse
                    setattr(self, key, Node(value))
                else:
                    
                    # add it to the existing Node
                    node.z_add(value)
            else:
                
                # make an attribute for the dict entry
                setattr(self, key, value)
                
    def z_getNode(self, name, lookDeeper):
        """ returns true if the name of the node is a child of this current node
            if lookDeeper is true, all underlying nodes will be searched
        """
        # print("looking for {}".format(name))
        for key, value in self.__dict__.iteritems():

            if lookDeeper == True and isinstance(value, Node):
                resp = value.z_getNode(name, True)
                if resp == True:
                    return resp
                
            if name == key:
                return value
        
        return False
        
    def z_hasAttr(self, name, lookDeeper):
        """ returns true if the attribute (value) is present in this node. 
            if lookDeeper is true, all underlying nodes will be searched
        """
    
    def z_all(self):
        """ return all values of all attributes 
        """
        return [value for key, value in sorted(self.__dict__.iteritems())]

    def z_addNode(self, name):
        """ Add a new node to this node, creating a tree
        """
        setattr(self, name, Node())
        return 
        
    def z_getDict(self, recurse=True, stringify=False):
        """ Get the dictionary representative of the node, and all underlying nodes
        """
        
        d = self.__dict__
        realDict = {}
        for key, value in d.iteritems():
            
            # recurse with a node if recurse == true 
            if isinstance(value, Node) and recurse:
                value = value.z_getDict(recurse, stringify)
            elif stringify:
                value = str(value)
                
            # copy the now correct entry
            realDict[key] = value
        
        # print(realDict)
        return realDict

    def z_replicate(self):
        
        return Node(self.z_getDict(True))

dv = Node(dvDict)
empty = Node()